using System;
using System.Collections.Generic;
using UnityEngine;

public class PokemonEgg
{
    public string OriginalTrainer { get; set; }
    public int HatchSteps { get; set; }
    public int CurrentSteps { get; set; }
    public Pokemon HatchingPokemon { get; set; }

    public PokemonEgg(Pokemon pokemon, int hatchSteps)
    {
        HatchingPokemon = pokemon;
        this.HatchSteps = hatchSteps;
        CurrentSteps = 0;
    }

    public bool IsHatched => CurrentSteps >= HatchSteps;

    public Pokemon Hatch()
    {
        Pokemon newPokemon = new Pokemon(HatchingPokemon.Base, 1);
        return newPokemon;
    }

    public int IncrementSteps(int steps) => CurrentSteps += steps;
}
