using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] ActionSelectionUI selectionUI;

    public ActionSelectionUI SelectionUI
    {
        get => selectionUI;
        set => selectionUI = value;
    }

    public int ActionIndex { get; set; }

    public static ActionSelectionState i { get; private set; }

    private void Awake() => i = this;

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        selectionUI.SelectedItem = 0;
        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnActionSelected; 
        
        bs.CurrentUnit = bs.PlayerUnits[ActionIndex];

        bs.DialogBox.SetDialog($"Choose an action for {bs.CurrentUnit.Pokemon.Base.Name}");
    }

    public override void Execute() => selectionUI.HandleUpdate();

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnActionSelected;
    }

    void OnActionSelected(int selection)
    {
        switch (selection)
        {
            case 0:
                StartCoroutine(GoToMoveSelection());
                break;
            case 1:
                StartCoroutine(GoToInventoryState());
                break;
            case 2:
                StartCoroutine(GoToPartyState());
                break;
            case 3:
                StartCoroutine(GoToRunAway());
                break;
        }

        selectionUI.SelectedItem = 0;
    }

    IEnumerator GoToRunAway()
    {
        var action = new BattleAction()
        {
            Type = ActionType.Run,
            User = bs.CurrentUnit
        };

        bs.Actions.Add(action);

        yield return bs.StateMachine.PushAndWait(RunTurnState.i);
    }

    public IEnumerator GoToMoveSelection()
    {
        // Adding comments to test source control
        bs.CurrentUnit = bs.PlayerUnits[ActionIndex];
        bs.DialogBox.SetDialog($"Choose an action for {bs.CurrentUnit.Pokemon.Base.Name}");
        MoveSelectionState.i.Moves = bs.CurrentUnit.Pokemon.Moves;
        yield return bs.StateMachine.PushAndWait(MoveSelectionState.i);
    }

    IEnumerator GoToPartyState()
    {
        yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;
        if (selectedPokemon != null)
        {
            bs.SelectedAction = ActionType.SwitchPokemon;
            bs.SelectedPokemon = selectedPokemon;

            var action = new BattleAction()
            {
                Type = ActionType.SwitchPokemon,
                User = BattleSystem.i.CurrentUnit,
                SelectedPokemon = selectedPokemon
            };

            yield return AddBattleAction(action);
        }
    }

    IEnumerator GoToInventoryState()
    {
        yield return GameController.i.StateMachine.PushAndWait(InventoryState.i);
        var selectedItem = InventoryState.i.SelectedItem;
        if (selectedItem != null)
        {
            bs.SelectedItem = selectedItem;

            var action = new BattleAction()
            {
                Type = ActionType.UseItem,
                User = BattleSystem.i.CurrentUnit
            };

            yield return AddBattleAction(action);
        }
    }

    public IEnumerator AddBattleAction(BattleAction action)
    {
        bs.Actions.Add(action);

        if (bs.Actions.Count == bs.UnitCount)
        {
            foreach (var enemyUnit in bs.EnemyUnits)
            {
                // Enemy Unit selects it's move then ADDs it to the Action List.
                var randAction = new BattleAction()
                {
                    Type = ActionType.Move,
                    User = enemyUnit,
                    Target = bs.PlayerUnits[UnityEngine.Random.Range(0, bs.PlayerUnits.Count)],
                    Move = enemyUnit.Pokemon.GetRandomMove()
                };

                bs.Actions.Add(randAction);
            }

            ActionIndex = 0;

            bs.Actions = bs.Actions.OrderByDescending(a => a.Priority).ThenByDescending(a => a.User.Pokemon.Speed).ToList();
            yield return bs.StateMachine.PushAndWait(RunTurnState.i);
        }
    }
}
