using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] ActionSelectionUI actionSelection;

    public static ActionSelectionState i { get; private set; }
    private void Awake() => i = this;

    // Outputs
    public int ActionIndex { get; set; }

    // References
    private BattleSystem _battleSystem;
    private Pokemon selectedPokemon;

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;

        actionSelection.SelectedItem = 0;
        actionSelection.gameObject.SetActive(true);
        actionSelection.OnSelected += OnActionSelected; 
        
        _battleSystem.CurrentUnit = _battleSystem.PlayerUnits[ActionIndex];
        _battleSystem.DialogBox.SetDialog($"Choose an action for {_battleSystem.PlayerUnits[ActionIndex].Pokemon.Base.Name}");
    }

    public override void Execute() => actionSelection.HandleUpdate();

    public override void Exit()
    {
        actionSelection.gameObject.SetActive(false);
        actionSelection.OnSelected -= OnActionSelected;
        ActionIndex = 0;
    }

    private void OnActionSelected(int selection)
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

        actionSelection.SelectedItem = 0;
    }

    private IEnumerator GoToRunAway()
    {
        var action = new BattleAction()
        {
            Type = ActionType.Run,
            User = _battleSystem.CurrentUnit
        };

        _battleSystem.Actions.Add(action);

        yield return _battleSystem.StateMachine.PushAndWait(RunTurnState.i);
    }

    public IEnumerator GoToMoveSelection()
    {
        actionSelection.gameObject.SetActive(false);

        yield return _battleSystem.StateMachine.PushAndWait(MoveSelectionState.i);
        var selectedMove = MoveSelectionState.i.Moves[MoveSelectionState.i.SelectedMove];
        actionSelection.SelectedItem = 0;
        if (selectedMove != null)
        {
            _battleSystem.CurrentUnit.Pokemon.CurrentMove = selectedMove;

            if (_battleSystem.UnitCount > 1)
            {
                yield return _battleSystem.StateMachine.PushAndWait(TargetSelectionState.i);

                var currentUnit = _battleSystem.PlayerUnits[ActionIndex];
                _battleSystem.CurrentUnit = currentUnit;

                _battleSystem.DialogBox.SetDialog($"Choose an action for {currentUnit.Pokemon.Base.Name}");
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

                AddBattleAction(action);
            }
        }

        actionSelection.gameObject.SetActive(true);
    }

    private IEnumerator GoToPartyState()
    {
        yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
        selectedPokemon = PartyState.i.SelectedPokemon;
        if (selectedPokemon != null)
        {
            var action = new BattleAction()
            {
                Type = ActionType.SwitchPokemon,
                User = BattleSystem.i.CurrentUnit,
                SelectedPokemon = selectedPokemon
            };

            AddBattleAction(action);
        }
    }

    private IEnumerator GoToInventoryState()
    {
        yield return GameController.i.StateMachine.PushAndWait(InventoryState.i);
        var selectedItem = InventoryState.i.SelectedItem;
        if (selectedItem != null)
        {
            _battleSystem.SelectedItem = selectedItem;

            var action = new BattleAction()
            {
                Type = ActionType.UseItem,
                User = BattleSystem.i.CurrentUnit
            };

            AddBattleAction(action);
        }
    }

    public void AddBattleAction(BattleAction action)
    {
        _battleSystem.Actions.Add(action);

        if (_battleSystem.Actions.Count == _battleSystem.UnitCount)
        {
            foreach (var enemyUnit in _battleSystem.EnemyUnits)
            {
                // Enemy Unit selects it's move then ADDs it to the Action List.
                var randAction = new BattleAction()
                {
                    Type = ActionType.Move,
                    User = enemyUnit,
                    Target = _battleSystem.PlayerUnits[UnityEngine.Random.Range(0, _battleSystem.PlayerUnits.Count)],
                    Move = enemyUnit.Pokemon.GetRandomMove()
                };

                _battleSystem.Actions.Add(randAction);
            }

            ActionIndex = 0;

            _battleSystem.Actions = _battleSystem.Actions.OrderByDescending(a => a.Priority).ThenByDescending(a => a.User.Pokemon.Speed).ToList();
            _battleSystem.StateMachine.Push(RunTurnState.i);
        }
    }
}
