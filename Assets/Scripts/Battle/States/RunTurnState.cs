using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RunTurnState : State<BattleSystem>
{
    public static RunTurnState i { get; private set; }
    private void Awake() => i = this;

    private bool isTrainerBattle;

    // Outputs
    public BattleUnit UnitToSwitch { get; set; }

    // References
    private List<BattleUnit> _playerUnits;
    private List<BattleUnit> _enemyUnits;
    private BattleDialog _dialogBox;
    private PokemonParty _playerParty;
    private PokemonParty _trainerParty;
    private BattleSystem _battleSystem;
    private ActionSelectionState _actionState;

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        _actionState = _battleSystem.GetComponent<ActionSelectionState>();

        _playerUnits = _battleSystem.PlayerUnits;
        _enemyUnits = _battleSystem.EnemyUnits;
        _dialogBox = _battleSystem.DialogBox;
        _playerParty = _battleSystem.PlayerParty;
        _trainerParty = _battleSystem.TrainerParty;

        isTrainerBattle = _battleSystem.IsTrainerBattle;

        StartCoroutine(RunTurns());
    }

    private IEnumerator RunTurns()
    {
        var actionsToProcess = new List<BattleAction>(_battleSystem.Actions);

        foreach (var action in actionsToProcess)
        {
            if (action.IsInvalid)
                continue;

            if (action.Type == ActionType.Move)
            {
                yield return RunMove(action.User, action.Target, action.Move);
                yield return RunAfterTurn(action.User, action.Target, action.Move);
            }
            else if (action.Type == ActionType.SwitchPokemon)
            {
                yield return _battleSystem.SwitchPokemon(action.User, action.SelectedPokemon);
            }
            else if (action.Type == ActionType.UseItem)
            {
                yield return UseItem();
            }
            else if(action.Type == ActionType.Run)
            {
                yield return TryToEscape();
            }

            if (_battleSystem.IsBattleOver)
            {
                yield break;
            }
        }

        yield return WeatherEffects();

        _battleSystem.Actions.Clear();
        _actionState.ActionIndex = 0;
        _battleSystem.StateMachine.Pop();
    }

    private IEnumerator UseItem()
    {
        if (_battleSystem.SelectedItem is PokeballItem)
        {
            yield return _battleSystem.ThrowPokeball(_battleSystem.SelectedItem as PokeballItem);
            if (_battleSystem.IsBattleOver) yield break;
        }
        else if (_battleSystem.SelectedItem is BattleItem)
        {
            var item = _battleSystem.SelectedItem as BattleItem;

            if (item.IsEscapeItem)
            {
                yield return _battleSystem.BattleItem(_battleSystem.SelectedItem as BattleItem); ;
                if (_battleSystem.IsBattleOver) yield break;
            }
        }
        else
        {
            // This is handled from item screen, so do nothing and skip to enemy move
        }
    }

    private IEnumerator WeatherEffects()
    {
        var weather = _battleSystem.Field.Weather;

        // Weather effects
        if (weather == null) yield break;

        yield return _dialogBox.TypeDialog(_battleSystem.Field.Weather.EffectMessage);

        // On Player Pokemon
        for (int i = 0; i < _battleSystem.UnitCount; i++)
        {
            yield return WeatherOnPokemonsTurn(_playerUnits[i], _enemyUnits[i]);
        }

        // On Enemy Pokemon
        for (int i = 0; i < _battleSystem.UnitCount; i++)
        {
            yield return WeatherOnPokemonsTurn(_enemyUnits[i], _playerUnits[i]);
        }

        if (_battleSystem.Field.WeatherDuration != null)
        {
            _battleSystem.Field.WeatherDuration--;
            if (_battleSystem.Field.WeatherDuration == 0)
            {
                _battleSystem.Field.Weather = null;
                _battleSystem.Field.WeatherDuration = null;
                yield return _dialogBox.TypeDialog("The weather has changed back to normal");
            }
        }
    }

    private IEnumerator WeatherOnPokemonsTurn(BattleUnit source, BattleUnit target)
    {
        var weather = _battleSystem.Field.Weather;

        if (source.Pokemon.Ability?.OnBlockWeather == null)
        {
            weather.OnWeather?.Invoke(source.Pokemon, target.Pokemon);
            if (source.Pokemon.HasHpChanged()) source.PlayHitAnimation();
            yield return ShowStatusChanges(source.Pokemon);
            yield return source.Hud.WaitForHPUpdate();
        }

        if (source.Pokemon.HP == 0)
        {
            yield return HandlePokemonFainted(source, _battleSystem.CurrentUnit, source.Pokemon.CurrentMove);
        }
    }

    private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        if (!CanRunMove(sourceUnit, targetUnit, move))
            yield break;

        move.PP--;

        if (sourceUnit.Pokemon.VolatileStatus?.Id != ConditionID.flinch)
            yield return _dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        targetUnit.Pokemon.Ability?.OnPreventMove?.Invoke(targetUnit.Pokemon, sourceUnit.Pokemon, move);

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
            yield return MoveHits(sourceUnit, targetUnit, move);
        else
            yield return _dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed");

        sourceUnit.Pokemon.PrevMoveUsed = move;

        sourceUnit.Pokemon.OnAfterMove(move, targetUnit.Pokemon, sourceUnit.Pokemon);
        sourceUnit.Pokemon.FirstTurn = false;
    }

    private IEnumerator MoveHits(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        var damageDetails = new DamageDetails();

        int hitCount = move.Base.GetHitCount();

        float typeEff = 1f;
        int hits = 1;

        for (int i = 1; i <= hitCount; i++)
        {
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1);

            if (move.Base.Target == MoveTarget.Self)
                sourceUnit.PlayHitAnimation();
            else
                targetUnit.PlayHitAnimation();

            AudioManager.i.PlaySfx(AudioID.Hit);

            // Healing move calculations 
            if (move.Base.Category == MoveCategory.Status)
            {
                if (move.Base.HealType != HealType.None)
                    yield return HealPokemon(sourceUnit, move);

                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target, move.Base);
            }
            else
            {
                //yield return InflictDamage(sourceUnit, targetUnit, move);
                damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon, _battleSystem.Field.Weather);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowCriticalHit(damageDetails);
                typeEff = damageDetails.TypeEffectiveness;
            }

            targetUnit.Pokemon.HeldItems?.OnHpChanged?.Invoke(targetUnit.Pokemon);

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                // yield return ApplySecondaryEffects(sourceUnit, targetUnit, move);
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target, move.Base);
                }
            }

            yield return RunAfterMove(damageDetails, move.Base, sourceUnit.Pokemon, targetUnit.Pokemon);

            hits = i;
            if (targetUnit.Pokemon.HP <= 0) 
                break;
        }

        yield return ShowEffectiveness(typeEff);

        if (hitCount > 1)
            yield return _dialogBox.TypeDialog($"Hit {hits} time(s)!");

        if (targetUnit.Pokemon.HP <= 0)
            yield return HandlePokemonFainted(targetUnit, sourceUnit, move);
    }

    private IEnumerator HealPokemon(BattleUnit sourceUnit, Move move)
    {
        int healAmount = CalculateHealAmount(sourceUnit.Pokemon.MaxHp, move.Base.HealType);
        sourceUnit.Pokemon.IncreaseHP(healAmount);

        yield return _dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} restored some health.");
    }

    private int CalculateHealAmount(int maxHp, HealType healType)
    {
        return healType switch
        {
            HealType.None => 0,
            HealType.Full => maxHp,
            HealType.Half => maxHp / 2,
            _ => 0,
        };
    }

    private bool CanRunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) => sourceUnit.Pokemon.OnBeforeMove(targetUnit.Pokemon, sourceUnit.Pokemon, move);

    private IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget, MoveBase move)
    {
        // Stat Boosting
        if (effects.Boosts != null)
        {
            var boostTarget = (moveTarget == MoveTarget.Self) ? source : target;
            boostTarget.ApplyBoosts(effects.Boosts.ToDictionary(x => x.stat, x => x.boost), source, move);
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            if (moveTarget == MoveTarget.Self)
                source.SetStatus(source, effects.Status, move.Effects);
            else
                target.SetStatus(source, effects.Status, move.Effects);
        }

        // Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            if (moveTarget == MoveTarget.Self)
                source.SetVolatileStatus(source, effects.VolatileStatus, move.Effects);
            else
                target.SetVolatileStatus(source, effects.VolatileStatus, move.Effects);
        }

        // Weather Condition
        if (effects.Weather != ConditionID.none)
        {
            _battleSystem.Field.SetWeather(source, effects.Weather);
            _battleSystem.Field.WeatherDuration = 5;
            yield return _dialogBox.TypeDialog(_battleSystem.Field.Weather.StartMessage);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);

        source.HeldItems?.OnStatusChanged?.Invoke(source);
        target.HeldItems?.OnStatusChanged?.Invoke(target);
    }

    private IEnumerator RunAfterMove(DamageDetails details, MoveBase move, Pokemon source, Pokemon target)
    {
        if (details == null)
            yield break;

        if (move.Recoil.recoilType != RecoilType.none)
        {
            int damage = 0;
            switch (move.Recoil.recoilType)
            {
                case RecoilType.RecoilByMaxHP:
                    int maxHp = source.MaxHp;
                    damage = Mathf.FloorToInt(maxHp * (move.Recoil.recoilDamage / 100f));
                    source.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByCurrentHP:
                    int currentHp = source.HP;
                    damage = Mathf.FloorToInt(currentHp * (move.Recoil.recoilDamage / 100f));
                    source.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByDamage:
                    damage = Mathf.FloorToInt(details.DamageDealt * (move.Recoil.recoilDamage / 100f));
                    source.TakeRecoilDamage(damage);
                    break;
                default:
                    Debug.Log("Error: Unknown Recoil Effect");
                    break;
            }
        }

        //ReUsed from status changes
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    private IEnumerator RunAfterTurn(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        if (_battleSystem.IsBattleOver) yield break;

        // Statuses like burn or psn will hurt the pokemon after the turn
        sourceUnit.Pokemon.OnAfterTurn(sourceUnit.Pokemon, targetUnit.Pokemon);
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        yield return targetUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit, targetUnit, move);
        }
    }

    private bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        moveAccuracy = source.ModifyAcc(moveAccuracy, source, target, move);

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    public IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return _dialogBox.TypeDialog(message);
        }
    }

    private IEnumerator HandlePokemonFainted(BattleUnit faintedUnit, BattleUnit sourceUnit, Move move)
    {
        yield return _dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} Fainted");
        faintedUnit.PlayFaintAnimation();
        faintedUnit.Pokemon.Ability?.OnFaintedPokemon?.Invoke(faintedUnit.Pokemon, sourceUnit.Pokemon, move);
        yield return new WaitForSeconds(2);

        yield return HandleExpGain(faintedUnit);

        yield return NextStepsAfterFainting(faintedUnit);
    }

    private IEnumerator HandleExpGain(BattleUnit faintedUnit)
    {
        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = isTrainerBattle && _trainerParty.GetHealthyPokemons(_battleSystem.UnitCount) == null;

            if (battleWon)
                AudioManager.i.PlayMusic(_battleSystem.BattleVictoryMusic);

            for (int i = 0; i < _battleSystem.UnitCount; i++)
            {
                var playerUnit = _playerUnits[i];

                playerUnit.Pokemon.GainEvs(faintedUnit.Pokemon.Base.EvYields);

                // Exp Gain
                int expGain = CalculateExpGain(faintedUnit);

                if (_playerUnits[0].Pokemon.HeldItem != null && _playerUnits[0].Pokemon.HeldItem.Name == "Lucky Egg")
                    expGain = Mathf.FloorToInt(expGain * 1.5f);

                playerUnit.Pokemon.Exp += expGain;
                yield return _dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} experience.");
                yield return playerUnit.Hud.SetExpSmooth();

                // Check Level Up
                while (playerUnit.Pokemon.CheckForLevelUp())
                {
                    yield return HandleLevelUp(_playerUnits[i]);

                    yield return playerUnit.Hud.SetExpSmooth();
                }
            }

            yield return new WaitForSeconds(1);
        }
    }

    private int CalculateExpGain(BattleUnit faintedUnit)
    {
        int expYield = faintedUnit.Pokemon.Base.ExpYield;
        int enemyLevel = faintedUnit.Pokemon.Level;
        float trainerBonus = (isTrainerBattle) ? 1.5f : 1;

        return Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / (7 * _battleSystem.UnitCount));
    }

    private IEnumerator HandleLevelUp(BattleUnit playerUnit)
    {
        playerUnit.Hud.SetLevel();

        yield return _dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}.");

        yield return TryLearnNewMove(playerUnit);
    }

    private IEnumerator TryLearnNewMove(BattleUnit playerUnit)
    {
        // Try to learn a new Move
        var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
        if (newMove != null)
        {
            if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumberOfMoves)
            {
                yield return LearnNewMove(newMove.Base, playerUnit);
            }
            else
            {
                // Option to forget a move.
                yield return _dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} wants to learn {newMove.Base.Name}");
                yield return _dialogBox.TypeDialog($"But it cannot learn more than {PokemonBase.MaxNumberOfMoves} moves.");
                yield return _dialogBox.TypeDialog($"Choose a move to forget.");

                MoveToForgetState.i.CurrentMoves = playerUnit.Pokemon.Moves.Select(m => m.Base).ToList();
                MoveToForgetState.i.NewMove = newMove.Base;
                yield return GameController.i.StateMachine.PushAndWait(MoveToForgetState.i);

                var moveIndex = MoveToForgetState.i.Selection;
                if (moveIndex == PokemonBase.MaxNumberOfMoves || moveIndex == -1)
                {
                    // Don't learn new move
                    yield return _dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} did not learn {newMove.Base.Name}.");
                }
                else
                {
                    // Forget old move and learn the new move
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    yield return _dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name} and learned {newMove.Base.Name}.");

                    playerUnit.Pokemon.Moves[moveIndex] = new Move(newMove.Base);
                }
            }
        }
    }

    private IEnumerator LearnNewMove(MoveBase newMove, BattleUnit playerUnit)
    {
        playerUnit.Pokemon.LearnMove(newMove);
        yield return _dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newMove.Name}.");
        _dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
    }

    private IEnumerator NextStepsAfterFainting(BattleUnit faintedUnit)
    {
        var actionToRemove = _battleSystem.Actions.FirstOrDefault(a => a.User == faintedUnit);
        if (actionToRemove != null)
            actionToRemove.IsInvalid = true;

        if (faintedUnit.IsPlayerUnit)
        {
            var activePokemons = _playerUnits.Select(u => u.Pokemon).Where(p => p.HP > 0).ToList();
            var nextPokemon = _playerParty.GetHealthyPokemon(activePokemons);

            if (activePokemons.Count == 0 && nextPokemon == null)
            {
                _battleSystem.BattleOver(false);
            }
            else if (nextPokemon != null)
            {
                // Send out next pokemon.
                UnitToSwitch = faintedUnit;
                yield return OpenPartyScreen();
            }
            else if (nextPokemon == null && activePokemons.Count > 0)
            {
                // No pokemon left to send out but we can still continue battle.
                _playerUnits.Remove(faintedUnit);
                faintedUnit.Hud.gameObject.SetActive(false);

                // Attacks targeting removed unit redirect to unit that is in battle.
                var actionsToChange = _battleSystem.Actions.Where(a => a.Target == faintedUnit).ToList();
                actionsToChange.ForEach(a => a.Target = _playerUnits.First());
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                _battleSystem.BattleOver(true);
                yield break; 
            }

            var activePokemons = _enemyUnits.Select(u => u.Pokemon).Where(p => p.HP > 0).ToList();
            var nextPokemon = _trainerParty.GetHealthyPokemon(activePokemons);

            if (activePokemons.Count == 0 && nextPokemon == null)
            {
                _battleSystem.BattleOver(true);
            }
            else if (nextPokemon != null)
            {
                if (_battleSystem.UnitCount == 1)
                {
                    UnitToSwitch = _playerUnits[0];

                    AboutToUseState.i.NewPokemon = nextPokemon;
                    yield return _battleSystem.StateMachine.PushAndWait(AboutToUseState.i);
                }
                else
                {
                    yield return _battleSystem.SendNextTrainerPokemon();
                }
            }
            else if (nextPokemon == null && activePokemons.Count > 0)
            {
                _enemyUnits.Remove(faintedUnit);
                faintedUnit.Hud.gameObject.SetActive(false);

                // Attacks targeting removed unit redirect to unit that is in battle.
                var actionsToChange = _battleSystem.Actions.Where(a => a.Target == faintedUnit).ToList();
                actionsToChange.ForEach(a => a.Target = _enemyUnits.First());
            }
        }
    }

    private IEnumerator OpenPartyScreen()
    {
        yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
        yield return _battleSystem.SwitchPokemon(_battleSystem.CurrentUnit, PartyState.i.SelectedPokemon);
    }

    private IEnumerator ShowCriticalHit(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return _dialogBox.TypeDialog("A critical hit!");
    }

    private IEnumerator ShowEffectiveness(float typeEff)
    {
        if (typeEff > 1f)
            yield return _dialogBox.TypeDialog("It's super effective!");
        else if (typeEff < 1f)
            yield return _dialogBox.TypeDialog("It's not very effective!");
    }

    private IEnumerator TryToEscape()
    {
        if (isTrainerBattle)
        {
            yield return _dialogBox.TypeDialog($"You can't run from a trainer battle!");
            yield break;
        }

        ++_battleSystem.escapeAttempts;

        int playerSpeed = _playerUnits[0].Pokemon.Speed;
        int enemySpeed = _enemyUnits[0].Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return _dialogBox.TypeDialog($"Ran away safely!");
            _battleSystem.BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * _battleSystem.escapeAttempts;
            f = f % 256;

            if (Random.Range(0, 256) < f)
            {
                yield return _dialogBox.TypeDialog($"Ran away safely.");
                _battleSystem.BattleOver(true);
            }
            else
                yield return _dialogBox.TypeDialog($"Can't escape!");
        }
    }
}
