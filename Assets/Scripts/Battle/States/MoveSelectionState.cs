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

    // Outputs
    public List<Move> Moves { get; private set; }
    public int SelectedMove { get; private set; }

    // References
    private Pokemon _pokemon;
    private Forms _forms;
    private BattleSystem _battleSystem;
    private ActionSelectionState actionState;

    public override void Enter(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;
        _battleSystem.DialogBox.EnableDialogText(false);

        actionState = _battleSystem.GetComponent<ActionSelectionState>();

        moveSelector.gameObject.SetActive(true);
        moveDetailsUI.SetActive(true);
        moveSelector.SetMoves(_battleSystem.PlayerUnits[actionState.ActionIndex].Pokemon.Moves);
        Moves = _battleSystem.CurrentUnit.Pokemon.Moves;

        moveSelector.OnSelected += OnMoveSelected;
        moveSelector.OnBack += OnBack;

        _pokemon = _battleSystem.CurrentUnit.Pokemon;
        _forms = _pokemon.CheckForMega(_pokemon.HeldItem);
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
        _battleSystem.StateMachine.Pop();
    }
    
    private void OnBack() => _battleSystem.StateMachine.Pop();

    public void HandleSelection()
    {
        if (_pokemon.Base.Forms.Count > 0)
        {
            StartCoroutine(HandleMega());
        }
        else
        {
            moveSelector.HandleUpdate();
        }
    }

    private IEnumerator HandleMega()
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

            _battleSystem.PokemonBeforeMega = _pokemon;
            _pokemon.isMega = true;
            canMega = false;
            megaEvoUI.SetActive(canMega);

            yield return _battleSystem.DialogBox.TypeDialog($"{_pokemon.Base.Name}'s {_pokemon.HeldItem.Name} is reacting to the Mega Bracelet!");
            yield return _battleSystem.CurrentUnit.MegaEvolve(_forms);
            yield return _battleSystem.DialogBox.TypeDialog($"{_pokemon.Base.Name} Mega Evolved!.");
            if (_pokemon.isMega) _battleSystem.CurrentUnit.Hud.MegaIcon.gameObject.SetActive(true);

            _battleSystem.StateMachine.Pop();
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
