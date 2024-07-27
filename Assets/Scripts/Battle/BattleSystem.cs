using DG.Tweening;
using PKMNUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum BattleTrigger { TallGrass, Water, Sand }

public class BattleSystem : MonoBehaviour
{
    #region SerializeFields
    [SerializeField] BattleUnit playerUnitSingle;
    [SerializeField] BattleUnit enemyUnitSingle;
    [SerializeField] List<BattleUnit> playerUnitsMulti;
    [SerializeField] List<BattleUnit> enemyUnitsMulti;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveToForgetSelection moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] GameObject singleBattleElements;
    [SerializeField] GameObject multiBattleElements;

    List<BattleUnit> playerUnits;
    List<BattleUnit> enemyUnits;


    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Battle Backgrounds")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Image playerPlot;
    [SerializeField] Image enemyPlot;
    [Header("Grass Battle Grounds")]
    [SerializeField] Sprite grassBg;
    [SerializeField] Sprite grassNightBg;
    [SerializeField] Sprite grassPlot;
    [SerializeField] Sprite grassNightPlot;
    [Header("Sand Battle Grounds")]
    [SerializeField] Sprite sandBg;
    [SerializeField] Sprite sandNightBg;
    [SerializeField] Sprite sandPlot;
    [SerializeField] Sprite sandNightPlot;
    [Header("Water Battle Grounds")]
    [SerializeField] Sprite waterBg;
    [SerializeField] Sprite waterPlot;
    #endregion

    #region Variables
    public static BattleSystem i { get; private set; }

    public List<BattleAction> Actions { get; set; }

    public int UnitCount { get; private set; } = 1;
    public BattleUnit CurrentUnit { get; set; }
    public Pokemon PokemonBeforeMega { get; set; }

    public StateMachine<BattleSystem> StateMachine { get; private set; }

    public event Action<bool> OnBattleOver;

    public int SelectedMove { get; set; }
    public ActionType SelectedAction { get; set; }
    public Pokemon SelectedPokemon { get; set; }
    public ItemBase SelectedItem { get; set; }

    public bool IsBattleOver { get; private set; }

    public PokemonParty PlayerParty { get; private set; }
    public PokemonParty TrainerParty { get; private set;}
    public Pokemon WildPokemon { get; private set;}

    public Field Field { get; set; }

    public bool IsTrainerBattle { get; private set; } = false;
    PlayerController player;
    public TrainerController Trainer { get; private set; }

    public int escapeAttempts { get; set; }

    BattleTrigger battleTrigger;
    #endregion

    private void Awake() => i = this;

    public IEnumerator StartBattle(PokemonParty playerParty, Pokemon wildPokemon,
        BattleTrigger trigger = BattleTrigger.TallGrass, int unitCount=1)
    {
        this.PlayerParty = playerParty;
        this.WildPokemon = wildPokemon;
        this.UnitCount = unitCount;

        player = playerParty.GetComponent<PlayerController>();
        IsTrainerBattle = false;

        ActionSelectionState.i.ActionIndex = 0;
        AudioManager.i.PlayMusic(wildBattleMusic);

        battleTrigger = trigger;
        yield return SetupBattle();
    }

    public IEnumerator StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty,
        BattleTrigger trigger = BattleTrigger.TallGrass, int unitCount=1)
    {
        this.PlayerParty = playerParty;
        this.TrainerParty = trainerParty;

        player = playerParty.GetComponent<PlayerController>();
        Trainer = trainerParty.GetComponent<TrainerController>();
        IsTrainerBattle = true;

        this.UnitCount = unitCount;

        AudioManager.i.PlayMusic(trainerBattleMusic);

        battleTrigger = trigger;
        yield return SetupBattle();
    }

    void SelectBattleGround(BattleTrigger trigger)
    {
        switch (trigger)
        {
            case BattleTrigger.TallGrass:
                backgroundImage.sprite = grassBg;
                playerPlot.sprite = grassPlot;
                enemyPlot.sprite = grassPlot;
                break;
            case BattleTrigger.Water:
                backgroundImage.sprite = waterBg;
                playerPlot.sprite = waterPlot;
                enemyPlot.sprite = waterPlot;
                break;
            case BattleTrigger.Sand:
                backgroundImage.sprite = sandBg;
                playerPlot.sprite = sandPlot;
                enemyPlot.sprite = sandPlot;
                break;
            default:
                break;
        }
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);

        // Setup Battle Elements
        singleBattleElements.SetActive(UnitCount == 1);
        multiBattleElements.SetActive(UnitCount > 1);

        if (UnitCount == 1)
        {
            playerUnits = new List<BattleUnit>() { playerUnitSingle };
            enemyUnits = new List<BattleUnit>() { enemyUnitSingle };
        }
        else
        {
            playerUnits = playerUnitsMulti.GetRange(0, playerUnitsMulti.Count);
            enemyUnits = enemyUnitsMulti.GetRange(0, enemyUnitsMulti.Count);
        }

        CurrentUnit = playerUnits[0];

        for (int i = 0; i < playerUnits.Count; i++)
        {
            playerUnits[i].Clear();
            enemyUnits[i].Clear();
        }
        
        SelectBattleGround(battleTrigger);

        if (IsTrainerBattle)
            yield return SetupTrainerBattle();
        else
            yield return SetupWildPokemonBattle();

        Field = new Field();

        // Use this to set weather at start of the battle.
        //Field.SetWeather(playerUnit.Pokemon, ConditionID.sandstorm);

        if (Field.Weather != null)
            yield return dialogBox.TypeDialog(Field.Weather.StartMessage);

        for (int i = 0; i < playerUnits.Count; i++)
            InvokePokemonAbilities(playerUnits[i].Pokemon, enemyUnits[i].Pokemon);

        for (int i = 0; i < playerUnits.Count; i++)
        {
            yield return RunTurnState.i.ShowStatusChanges(playerUnits[i].Pokemon);
            yield return RunTurnState.i.ShowStatusChanges(enemyUnits[i].Pokemon);
        }
        
        ResetBattleState();

        Actions = new List<BattleAction>();
        StateMachine.Push(ActionSelectionState.i);
    }

    void InvokePokemonAbilities( Pokemon playerUnit, Pokemon enemyUnit)
    {
        InvokeAbility(enemyUnit, playerUnit);
        InvokeAbility(playerUnit, enemyUnit);
    }

    void InvokeAbility(Pokemon source, Pokemon target) => source.Ability?.OnPokemonEnter?.Invoke(source, target);

    void ResetBattleState()
    {
        IsBattleOver = false;
        escapeAttempts = 0;
        partyScreen.Init();
    }

    private IEnumerator SetupWildPokemonBattle()
    {
        // Wild Pokemon Battle
        playerUnits[0].Setup(PlayerParty.GetHealthyPokemon());
        enemyUnits[0].Setup(WildPokemon);

        dialogBox.SetMoveNames(playerUnits[0].Pokemon.Moves);
        yield return dialogBox.TypeDialog($"A wild {enemyUnits[0].Pokemon.Base.Name} appeared.");

        if (enemyUnits[0].Pokemon.HeldItem == null)
            Debug.Log($"{enemyUnits[0].Pokemon.Base.name} Held Item - NO ITEM");
        else
            Debug.Log($"{enemyUnits[0].Pokemon.Base.name} Held Item - {enemyUnits[0].Pokemon.HeldItem.Name.ToUpper()}");

        Debug.Log($"{enemyUnits[0].Pokemon.Base.name} ability is {enemyUnits[0].Pokemon.Ability.Name.ToUpper()}");

        Debug.Log($"{playerUnits[0].Pokemon.Base.name} Held Item - {playerUnits[0].Pokemon.HeldItem.Name.ToUpper()}");
        Debug.Log($"{playerUnits[0].Pokemon.Base.name} ability is {playerUnits[0].Pokemon.Ability.Name.ToUpper()}");
    }

    private IEnumerator SetupTrainerBattle()
    {
        // Show trainer and player sprites.
        for (int i = 0; i < playerUnits.Count; i++)
        {
            playerUnits[i].gameObject.SetActive(false);
            enemyUnits[i].gameObject.SetActive(false);
        }
        
        playerImage.gameObject.SetActive(true);
        trainerImage.gameObject.SetActive(true);
        playerImage.sprite = player.Sprite;
        trainerImage.sprite = Trainer.Sprite;

        yield return dialogBox.TypeDialog($"{Trainer.Name} wants to battle");

        // Send out first pokemon of trainer
        trainerImage.gameObject.SetActive(false);
        var enemyPokemons = TrainerParty.GetHealthyPokemons(UnitCount);
        
        for (int i = 0; i < UnitCount; i++)
        {
            enemyUnits[i].gameObject.SetActive(true);
            enemyUnits[i].Setup(enemyPokemons[i]);
        }

        string names = String.Join(" and ", enemyPokemons.Select(p => p.Base.Name));
        yield return dialogBox.TypeDialog($"{Trainer.Name} sent out {names}.");

        // Send out first pokemon of the player
        playerImage.gameObject.SetActive(false);
        var playerPokemons = PlayerParty.GetHealthyPokemons(UnitCount);

        for (int i = 0; i < UnitCount; i++)
        {
            playerUnits[i].gameObject.SetActive(true);
            playerUnits[i].Setup(playerPokemons[i]);
        }

        names = String.Join(" and ", playerPokemons.Select(p => p.Base.Name));
        yield return dialogBox.TypeDialog($"Go {names}!");
    }

    public void BattleOver(bool won)
    {
        IsBattleOver = true;
        PlayerParty.Pokemons.ForEach(p => p.OnBattleOver());

        playerUnits.ForEach(u => u.Hud.ClearData());
        EnemyUnits.ForEach(u => u.Hud.ClearData());

        OnBattleOver(won);
    }

    public void HandleUpdate() => StateMachine.Execute();

    public IEnumerator SwitchPokemon(BattleUnit unitToSwitch, Pokemon newPokemon)
    {
        if (unitToSwitch.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {unitToSwitch.Pokemon.Base.Name}.");
            unitToSwitch.PlayFaintAnimation();
            yield return new WaitForSeconds(2);
        }

        unitToSwitch.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        for (int i = 0; i < UnitCount; i++)
        {
            playerUnits[i].Pokemon.Ability?.OnPokemonEnter?.Invoke(playerUnits[i].Pokemon, enemyUnits[i].Pokemon);
        }

        yield return RunTurnState.i.ShowStatusChanges(newPokemon);
    }

    public IEnumerator SendNextTrainerPokemon()
    {
        var faintedUnit = enemyUnits.First(u => u.Pokemon.HP == 0);

        var activePokemons = enemyUnits.Select(u => u.Pokemon).Where(p => p.HP > 0).ToList();
        var nextPokemon = TrainerParty.GetHealthyPokemon(activePokemons);
        faintedUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{Trainer.Name} sent out {nextPokemon.Base.Name}!");

        nextPokemon.Ability?.OnPokemonEnter?.Invoke(nextPokemon, CurrentUnit.Pokemon);
        yield return RunTurnState.i.ShowStatusChanges(nextPokemon);

        RunTurnState.i.UnitToSwitch = null;
    }

    public IEnumerator BattleItem(BattleItem battleItem)
    {
        if (IsTrainerBattle && battleItem.IsEscapeItem)
        {
            yield return dialogBox.TypeDialog($"You can't escape from a trainer battle!");
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used a {battleItem.Name.ToUpper()}!");

        if (battleItem.IsEscapeItem)
        {
            yield return dialogBox.TypeDialog($"Wild {enemyUnits[0].Pokemon.Base.Name} was distracted while you escaped!");
            BattleOver(true);        
        }
    }
    
    public IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal other trainers Pokemon!");
            yield break;
        }

        var playerUnit = playerUnits[0];
        var enemyUnit = enemyUnits[0];

        yield return dialogBox.TypeDialog($"{player.Name} used a {pokeballItem.Name.ToUpper()}!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        // Animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(-0.3f, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 2.7f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon, pokeballItem);

        for (int i = 0; i < MathF.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Pokemon Caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            PlayerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party.");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            // Pokemon Broke Free
            yield return new WaitForSeconds(1);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free!");
            else
                yield return dialogBox.TypeDialog($"Almost caught it.");

            Destroy(pokeball);
        }
    }

    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeball)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * pokeball.CatchRateModifier * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / MathF.Sqrt(MathF.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    private void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = 24;
        style.alignment = TextAnchor.UpperRight;

        float screenWidth = Screen.width;
        float areaWidth = 300f;

        GUILayout.BeginArea(new Rect(screenWidth - areaWidth, 0, areaWidth, Screen.height));

        GUILayout.Label("BATTLE STATE STACK", style);
        foreach (var state in StateMachine.StateStack)
        {
            GUILayout.Label(state.GetType().ToString(), style);
        }

        GUILayout.EndArea();
    }

    public List<BattleUnit> PlayerUnits => playerUnits;
    public List<BattleUnit> EnemyUnits => enemyUnits;
    public BattleDialog DialogBox => dialogBox;
    public PartyScreen PartyScreen => partyScreen;
    public AudioClip BattleVictoryMusic => battleVictoryMusic;
}