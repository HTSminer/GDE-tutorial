using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TargetSelectionState : State<BattleSystem>
{
    [SerializeField] TargetSelectionUI selectionUI;

    public static TargetSelectionState i { get; private set; }
    private void Awake() => i = this;

    private int currentTarget = 0;

    // References
    private BattleSystem _battleSystem;
    private ActionSelectionState _actionState;

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        _actionState = _battleSystem.GetComponent<ActionSelectionState>();
        currentTarget = 0;
    }

    public override void Execute() => HandleSelection();

    public override void Exit()
    {
        _battleSystem.EnemyUnits[currentTarget].SetSelected(false);
        _actionState.ActionIndex++;
    }

    private void HandleSelection()
    {
        if (Input.GetKeyDown(KeyCode.D))
            ++currentTarget;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentTarget;

        currentTarget = Mathf.Clamp(currentTarget, 0, _battleSystem.EnemyUnits.Count - 1);

        for (int i = 0; i < _battleSystem.EnemyUnits.Count; i++)
        {
            _battleSystem.EnemyUnits[i].SetSelected(i == currentTarget);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var action = new BattleAction()
            {
                Type = ActionType.Move,
                User = _battleSystem.CurrentUnit,
                Target = _battleSystem.EnemyUnits[currentTarget],
                Move = _battleSystem.CurrentUnit.Pokemon.CurrentMove
            };

            _battleSystem.StateMachine.Pop();
            _actionState.ActionIndex = Mathf.Clamp(_actionState.ActionIndex++, 0, _battleSystem.UnitCount - 1);
            StartCoroutine(_actionState.AddBattleAction(action));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            _battleSystem.EnemyUnits[currentTarget].SetSelected(false);
            _battleSystem.StateMachine.Pop();
        }
    }
}
