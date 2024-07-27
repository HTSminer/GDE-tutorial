using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandEncounter : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameController.i.StartBattle(BattleTrigger.Sand);
        }
    }

    public bool TriggerRepeatedly => true;
}
