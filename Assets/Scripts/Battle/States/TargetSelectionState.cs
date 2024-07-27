using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TargetSelectionState : State<BattleSystem>
{
    [SerializeField] TargetSelectionUI selectionUI;

    private int currentTarget = 0;

    public static TargetSelectionState i { get; private set; }

    private void Awake() => i = this;

    BattleSystem bs;
    ActionSelectionState actionState;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        actionState = bs.GetComponent<ActionSelectionState>();
        currentTarget = 0;
    }

    public override void Execute()
    {
        HandleSelection();
    }

    public override void Exit()
    {
        actionState.SelectionUI.SelectedItem = 0;
        actionState.SelectionUI.gameObject.SetActive(false);
        bs.EnemyUnits[currentTarget].SetSelected(false);

        StartCoroutine(actionState.GoToMoveSelection());
    }

    private void HandleSelection()
    {
        if (Input.GetKeyDown(KeyCode.D))
            ++currentTarget;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentTarget;

        currentTarget = Mathf.Clamp(currentTarget, 0, bs.EnemyUnits.Count - 1);

        for (int i = 0; i < bs.EnemyUnits.Count; i++)
        {
            bs.EnemyUnits[i].SetSelected(i == currentTarget);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bs.EnemyUnits[currentTarget].SetSelected(false);

            var action = new BattleAction()
            {
                Type = ActionType.Move,
                User = bs.CurrentUnit,
                Target = bs.EnemyUnits[currentTarget],
                Move = bs.CurrentUnit.Pokemon.Moves[bs.SelectedMove]
            };

            StartCoroutine(ActionSelectionState.i.AddBattleAction(action));
            ActionSelectionState.i.ActionIndex++;
            bs.StateMachine.Pop();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            bs.EnemyUnits[currentTarget].SetSelected(false);
        }
    }
}
