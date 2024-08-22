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

    public static MoveSelectionState i { get; private set; }
    private void Awake() => i = this;

    private bool canMega;
    public Pokemon PokemonBeforeMega { get; set; }

    // Outputs
    public List<Move> Moves { get; private set; }
    public int SelectedMove { get; private set; }

    // References
    private Pokemon _pokemon;
    private BattleSystem _battleSystem;
    private ActionSelectionState actionState;

    public override void Enter(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;
        _battleSystem.DialogBox.EnableDialogText(false);

        actionState = _battleSystem.GetComponent<ActionSelectionState>();

        moveSelector.SetMoves(_battleSystem.PlayerUnits[actionState.ActionIndex].Pokemon.Moves);
        moveSelector.gameObject.SetActive(true);
        moveSelector.OnSelected += OnMoveSelected;
        moveSelector.OnBack += OnBack;

        moveDetailsUI.SetActive(true);

        Moves = _battleSystem.CurrentUnit.Pokemon.Moves;

        _pokemon = _battleSystem.CurrentUnit.Pokemon;
    }

    public override void Execute() => HandleSelection();

    public override void Exit()
    {
        _battleSystem.DialogBox.EnableDialogText(true);
        moveSelector.gameObject.SetActive(false);
        moveDetailsUI.SetActive(false);

        moveSelector.OnSelected -= OnMoveSelected;
        moveSelector.OnBack -= OnBack;

        moveSelector.ClearItems();
    }

    private void OnMoveSelected(int selected)
    {
        SelectedMove = selected;
        _battleSystem.StateMachine.ChangeState(RunTurnState.i);
    }

    private void OnBack()
    {
        _battleSystem.StateMachine.ChangeState(ActionSelectionState.i);
    }

    public void HandleSelection()
    {
        if (_pokemon.Base.Forms.Count > 0)
        {
            HandleMega();
        }
        
        moveSelector.HandleUpdate();
    }

    private void HandleMega()
    {
        if (!_pokemon.isMega && Input.GetKeyDown(KeyCode.LeftShift))
        {
            megaEvoUI.SetActive(ToggleMegaEvo(canMega));
        }
        else if (canMega && Input.GetKeyDown(KeyCode.Z))
        {
            moveSelector.gameObject.SetActive(false);
            moveDetailsUI.SetActive(false);
            _battleSystem.DialogBox.EnableDialogText(true);

            var action = new BattleAction()
            {
                Type = ActionType.MegaEvolve,
                User = _battleSystem.CurrentUnit
            };

            ActionSelectionState.i.AddBattleAction(action);

            megaEvoUI.SetActive(canMega);
        }
    }

    private bool ToggleMegaEvo(bool evo)
    {
        if (!evo)
        {
            canMega = true;
            megaEvoUI.GetComponent<TMP_Text>().color = Color.red;
        }
        else
            canMega = false;

        return canMega;
    }
}
