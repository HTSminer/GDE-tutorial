using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Pokemon pokemonToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GivePokemon(PlayerController player)
    {
        yield return DialogManager.i.ShowDialog(dialog);

        pokemonToGive.Init();
        player.GetComponent<PokemonParty>().AddPokemon(pokemonToGive);

        used = true;

        AudioManager.i.PlaySfx(AudioID.pokemonObtained, pauseMusic: true);

        string dialogText = $"{player.Name} received {pokemonToGive.Base.Name}.";

        yield return DialogManager.i.ShowDialogText(dialogText);
    }

    public bool CanBeGiven() => pokemonToGive != null && !used;

    public object CaptureState() => used;

    public void RestoreState(object state) => used = (bool)state;
}
