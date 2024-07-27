using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;

        yield return DialogManager.i.ShowDialogText("You look tired! Let's heal those Pokemon!",
            choices: new List<string>() { "Yes", "No" }, 
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // Yes
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal(p.MaxHp));
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.i.ShowDialogText($"Your pokemon should be fully healed now.");
        }
        else if (selectedChoice == 1)
        {
            // No
            yield return DialogManager.i.ShowDialogText($"Okay! Come back if you change your mind.");
        }
    }
}
