using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurfableWater : MonoBehaviour, Interactable, IPlayerTriggerable
{
    bool IsJumpingToWater = false;

    public bool TriggerRepeatedly => true;

    public IEnumerator Interact(Transform initiator)
    {
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator.IsSurfing || IsJumpingToWater)
            yield break;

        yield return DialogManager.i.ShowDialogText($"The water is deep blue.");

        var pokemonWithSurf = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Surf"));

        int selectedChoice = 0;
        if (pokemonWithSurf != null)
        {
            yield return DialogManager.i.ShowDialogText($"Should {pokemonWithSurf.Base.Name} use surf?.",
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                // Yes
                yield return DialogManager.i.ShowDialogText($"{pokemonWithSurf.Base.Name} used surf!");

                var dir = new Vector3(animator.MoveX, animator.MoveY) * 2;
                var targetPos = initiator.position + dir;

                IsJumpingToWater = true;
                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                IsJumpingToWater = false;

                animator.IsSurfing = true;
            }
        }
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 10)
        {
            GameController.i.StartBattle(BattleTrigger.Water);
        }
    }
}
