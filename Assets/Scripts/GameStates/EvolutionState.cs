using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PKMNUtils.StateMachine;
using System.Threading.Tasks;

public class EvolutionState : State<GameController>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    [SerializeField] AudioClip evolutionStart;
    [SerializeField] AudioClip evolutionMusic;
    [SerializeField] AudioClip evolutionComplete;

    Pokemon pokemonCopy;

    public static EvolutionState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        var gc = GameController.i;
        gc.StateMachine.Push(this);

        evolutionUI.SetActive(true);

        AudioManager.i.PlayMusic(evolutionStart, loop: false);

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.i.ShowDialogText($"{pokemon.Base.name} is evolving.");

        var oldPokemon = pokemon.Base;
        pokemon.Evolve(evolution);

        yield return PlayEvolveAnimation(oldPokemon, pokemon.Base);
        AudioManager.i.PlayMusic(evolutionComplete, loop: false);
        yield return new WaitForSeconds(4f);
        
        yield return DialogManager.i.ShowDialogText($"{oldPokemon.name} evolved into {pokemon.Base.name}.");
        evolutionUI.SetActive(false);

        gc.PartyScreen.SetPartyData();
        AudioManager.i.PlayMusic(gc.CurrentScene.SceneMusic, fade: true);

        pokemon.hasLeveled = false;
        gc.StateMachine.Pop();
    }

    private IEnumerator PlayEvolveAnimation(PokemonBase pokemon, PokemonBase evolution)
    {
        AudioManager.i.PlayMusic(evolutionMusic);

        var sequence = DOTween.Sequence();
        int cycles = 20;
        float speed = 1f;

        sequence.Append(pokemonImage.DOColor(Color.black, 1));

        // Shrink; Change Image; Grow; Repeat;
        for (int i = 1; i < cycles; i++)
        {
            sequence.Append(pokemonImage.transform.DOScale(0, speed / (float)i));

            if (i % 2 == 1)
                sequence.AppendCallback(() => pokemonImage.sprite = evolution.FrontSprite);
            else
                sequence.AppendCallback(() => pokemonImage.sprite = pokemon.FrontSprite);

            sequence.Append(pokemonImage.transform.DOScale(1, speed / (float)i));
        }

        sequence.AppendCallback(() => pokemonImage.sprite = evolution.FrontSprite);
        sequence.Join(pokemonImage.DOColor(Color.white, 1));

        yield return sequence.WaitForCompletion();
    }
}
