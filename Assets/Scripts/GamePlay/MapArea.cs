using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<PokemonEncounterRecord> wildPokemons;
    [SerializeField] List<PokemonEncounterRecord> wildPokemonsInWater;

 #region For Editor
    [HideInInspector]
    [SerializeField] int totalChanceDay;
    [HideInInspector]
    [SerializeField] int totalChanceNight;
    [HideInInspector]
    [SerializeField] int totalChanceWater;
    
    List<PokemonEncounterRecord> pokemonList;

    private void OnValidate()
    {
        CalculateChancePercentage();
    }
 #endregion

    private void Start()
    {
        CalculateChancePercentage();
    }

    void CalculateChancePercentage()
    {
        CalculateTotalChance(ref totalChanceDay, wildPokemons, TimeOfDay.Day);
        CalculateTotalChance(ref totalChanceNight, wildPokemons, TimeOfDay.Night);
        CalculateTotalChance(ref totalChanceWater, wildPokemonsInWater, TimeOfDay.None);
    }

    private void CalculateTotalChance(ref int totalChance, List<PokemonEncounterRecord> records, TimeOfDay timeOfDay)
    {
        totalChance = -1;

        if (records.Count > 0)
        {
            totalChance = 0;
            foreach (var record in records.Where(r => r.timeOfDay == timeOfDay || timeOfDay == TimeOfDay.None))
            {
                record.chanceLower = totalChance;
                record.chanceUpper = totalChance + record.chancePercentage;
                totalChance += record.chancePercentage;
            }
        }
    }

    public Pokemon GetRandomWildPokemon(BattleTrigger trigger)
    {
        switch (trigger)
        {
            case BattleTrigger.TallGrass:
                pokemonList = wildPokemons.Where(e => e.timeOfDay == DayNightCycle.i.DayOrNight).ToList();
                break;
            case BattleTrigger.Water:
                pokemonList = wildPokemonsInWater;
                break;
            default:
                break;
        }
        
        int randVal = Random.Range(1, 101);
        var pokemonRecord = pokemonList.First(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);

        var levelRange = pokemonRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);

        var wildPokemon = new Pokemon(pokemonRecord.pokemon, level);
        wildPokemon.Init();
        return wildPokemon;
    }
}

[System.Serializable]
public class PokemonEncounterRecord
{
    public PokemonBase pokemon;
    public Vector2Int levelRange;
    public int chancePercentage;
    public TimeOfDay timeOfDay;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }
}