using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;
    [SerializeField] List<PokemonEgg> eggs;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
        set { pokemons = value; OnUpdated?.Invoke(); }
    }

    public List<PokemonEgg> Eggs
    {
        get { return eggs; }
        set { eggs = value; OnUpdated?.Invoke(); }
    }

    public static PokemonParty i { get; private set; }

    private void Awake()
    {
        i = this;
        Eggs = new List<PokemonEgg>();
        foreach (var pokemon in pokemons)
            pokemon.Init();
    }

    public Pokemon GetHealthyPokemon(List<Pokemon> dontInclude=null)
    {
        var healthyPokemons = pokemons.Where(x => x.HP > 0);
        if (dontInclude != null)
            healthyPokemons = healthyPokemons.Where(p => !dontInclude.Contains(p));

        return healthyPokemons.FirstOrDefault();
    }

    public List<Pokemon> GetHealthyPokemons(int unitCount) => pokemons.Where(x => x.HP > 0).Take(unitCount).ToList();

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            // Transfer to PC
        }
    }

    public void RemovePokemon(Pokemon newPokemon)
    {
        pokemons.Remove(newPokemon);
        OnUpdated?.Invoke();
    }

    public void AddEgg(PokemonEgg egg)
    {
        eggs.Add(egg);
        OnUpdated?.Invoke();
    }

    public void RemoveEgg(PokemonEgg egg, Pokemon pokemon)
    {
        eggs.Remove(egg);
        pokemons.Remove(pokemon);
        OnUpdated?.Invoke();
    }

    public bool CheckForEvolutions() => pokemons.Any(p => p.CheckForEvolution() != null);

    public IEnumerator RunEvolutions()
    {
        foreach (var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
                yield return EvolutionState.i.Evolve(pokemon, evolution);
        }
    }

    public void PartyUpdated() => OnUpdated?.Invoke();

    public static PokemonParty GetPlayerParty() => FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
}
