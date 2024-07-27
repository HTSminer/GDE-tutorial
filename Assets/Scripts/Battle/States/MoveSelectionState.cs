using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MoveSelectionState : State<BattleSystem>
{
    [SerializeField] MoveSelectionUI moveSelector;
    [SerializeField] GameObject moveDetailsUI;
    [SerializeField] GameObject megaEvoUI;

    public bool CanMega { get; private set; }

    public Pokemon pokemon { get; private set; } 
    public Forms mega { get; private set; }

    // Inputs
    public List<Move> Moves { get; set; }

    public static MoveSelectionState i { get; private set; }

    private void Awake() => i = this;

    BattleSystem _battleSystem;
    public override void Enter(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;
        _battleSystem.DialogBox.EnableDialogText(false);

        moveSelector.gameObject.SetActive(true);
        moveDetailsUI.SetActive(true);
        moveSelector.SetMoves(_battleSystem.PlayerUnits[0].Pokemon.Moves);

        moveSelector.OnSelected += OnMoveSelected;
        moveSelector.OnBack += OnBack;

        pokemon = _battleSystem.CurrentUnit.Pokemon;
        mega = pokemon.CheckForMega(pokemon.HeldItem);
    }

    public override void Execute() => StartCoroutine(HandleUpdate());

    public override void Exit()
    {
        _battleSystem.DialogBox.EnableDialogText(true);
        moveSelector.gameObject.SetActive(false);
        moveDetailsUI.SetActive(false);

        moveSelector.OnSelected -= OnMoveSelected;
        moveSelector.OnBack -= OnBack;

        moveSelector.ClearItems();
    }

    public IEnumerator HandleUpdate()
    {
        if (pokemon.Base.Forms.Count > 0)
        {
            if (!pokemon.isMega && Input.GetKeyDown(KeyCode.LeftShift))
                megaEvoUI.SetActive(ToggleMegaEvo(CanMega, pokemon, mega));
            else if (CanMega && Input.GetKeyDown(KeyCode.Z))
            {
                _battleSystem.PokemonBeforeMega = pokemon;

                moveSelector.gameObject.SetActive(false);
                moveDetailsUI.SetActive(false);
                _battleSystem.DialogBox.EnableDialogText(true);

                pokemon.isMega = true;
                CanMega = false;

                megaEvoUI.SetActive(CanMega);

                yield return _battleSystem.DialogBox.TypeDialog($"{pokemon.Base.Name}'s {pokemon.HeldItem.Name} is reacting to the Mega Bracelet!");
                yield return _battleSystem.CurrentUnit.MegaEvolve(mega);
                yield return _battleSystem.DialogBox.TypeDialog($"{pokemon.Base.Name} Mega Evolved!.");
                if (pokemon.isMega) _battleSystem.CurrentUnit.Hud.MegaIcon.gameObject.SetActive(true);

                _battleSystem.StateMachine.Pop();
            }
        }
        else
            moveSelector.HandleUpdate();
    }

    void OnMoveSelected(int selected)
    {
        moveSelector.gameObject.SetActive(false);
        moveDetailsUI.SetActive(false);
        _battleSystem.DialogBox.EnableDialogText(true);

        var selectedMove = Moves[selected];
        if (selectedMove != null)
        {
            _battleSystem.SelectedAction = ActionType.Move;
            _battleSystem.CurrentUnit.Pokemon.CurrentMove = selectedMove;

            if (_battleSystem.UnitCount > 1)
            {
                StartCoroutine(_battleSystem.StateMachine.PushAndWait(TargetSelectionState.i));
            }
            else
            {
                var action = new BattleAction()
                {
                    Type = ActionType.Move,
                    User = _battleSystem.CurrentUnit,
                    Target = _battleSystem.EnemyUnits[0],
                    Move = selectedMove
                };
                StartCoroutine(ActionSelectionState.i.AddBattleAction(action));
                ActionSelectionState.i.ActionIndex = Mathf.Clamp(ActionSelectionState.i.ActionIndex++, 0, _battleSystem.UnitCount);
            }
        }
    }
    
    void OnBack() => _battleSystem.StateMachine.Pop();

    bool ToggleMegaEvo(bool evo, Pokemon pokemon, Forms mega)
    {
        if (!evo)
        {
            CanMega = true;
            megaEvoUI.GetComponent<TMP_Text>().color = Color.red;
        }
        else
            CanMega = false;

        return CanMega;
    }
}
