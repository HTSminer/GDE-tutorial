using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuttableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.i.ShowDialogText($"This tree looks like it can be cut.");

        var pokemonWithCut = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Cut"));

        int selectedChoice = 0;
        if (pokemonWithCut != null)
        {
            yield return DialogManager.i.ShowDialogText($"Should {pokemonWithCut.Base.Name} use cut?.",
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                // Yes
                yield return DialogManager.i.ShowDialogText($"{pokemonWithCut.Base.Name} used cut.");
                gameObject.SetActive(false);
            }
        }
    }
}
