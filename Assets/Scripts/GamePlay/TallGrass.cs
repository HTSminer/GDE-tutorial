using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TallGrass : MonoBehaviour, IPlayerTriggerable
{
    public static TallGrass i { get; private set; }

    public bool halfEncounter = false;

    private int encounterRate;

    private void Awake() => i = this;

    public void OnPlayerTriggered(PlayerController player)
    {
        encounterRate = Random.Range(1, 101);

        var ability = player.Party.Pokemons[0].Ability;
        if (ability?.OutsideBattle != null)
        {
            halfEncounter = ability.OutsideBattle(player.Party.Pokemons[0]);
            
            if (halfEncounter)
                encounterRate = encounterRate * 2;

            Debug.Log($"Stench active: {encounterRate}");
        }
        
        if (encounterRate <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameController.i.StartBattle(BattleTrigger.TallGrass);
        }
    }

    public bool TriggerRepeatedly => true;
}
