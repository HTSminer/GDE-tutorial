using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : EffectData
{
    public AbilityID Id { get; set; }
    public string Name { get; set; }
    public EffectData Effects { get; set; }
    public string Description { get; set; }

    public Action<Dictionary<Stat, int>, Pokemon, Pokemon> OnBoost { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnBasePower { get; set; }

    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAtk { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyDef { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpAtk { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpDef { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpd { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAcc { get; set; }


    public Action<Pokemon> OnBeforeTurn { get; set; }
    public Action<int, Pokemon, Pokemon, EffectData> OnBeforeMove { get; set; }
    public Func<float, Pokemon, Pokemon, Move, int> OnBeforeDamage { get; set; }
    public Action<float, Pokemon, Pokemon, Move> OnDamagingHit { get; set; }
    public Action<int, Pokemon, Pokemon, EffectData> OnAfterMove { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }

    public Func<ConditionID, Pokemon, Pokemon, EffectData, bool> OnTrySetVolatile { get; set; }
    public Func<ConditionID, Pokemon, Pokemon, EffectData, bool> OnTrySetStatus { get; set; }

    public Func<Pokemon, Pokemon, Move, bool> OnPreventMove { get; set; }
    public Func<Pokemon, bool> OnBlockWeather { get; set; }

    public Func<Pokemon, Pokemon, bool> OnTrapped { get; set; }
    public Action<Pokemon, Pokemon, Move> OnFaintedPokemon { get; set; }
    public Action<Pokemon, Pokemon> OnPokemonEnter { get; set; }
    public Action<Pokemon> OnOpponentStatus { get; set; }

    public Func<Pokemon, bool> OutsideBattle { get; set; }
}
