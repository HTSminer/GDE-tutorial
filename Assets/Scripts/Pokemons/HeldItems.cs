using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldItems : EffectData
{
    public HeldItemID Id { get; set; }
    public string Name { get; set; }
    public Func<Pokemon, Pokemon, string> StartMessage { get; set; }
    public MoveEffects Effects { get; set; }
    public string Description { get; set; }

    public Action<Dictionary<Stat, int>, Pokemon, Pokemon> OnBoost { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnBasePower { get; set; }
    public Action<Pokemon, Pokemon, Move> OnDamagingHit { get; set; }
    public Func<int, Pokemon, Move, int> OnTakeDamage { get; set; }
    public Action<Pokemon> OnHpChanged { get; set; }
    public Action<Pokemon>OnStatusChanged { get; set; }

    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAtk { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyDef { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpAtk { get; set; }
    public Func<float, Pokemon, float> OnModifySpDef { get; set; }
    public Func<float, Pokemon, float> OnModifySpd { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAcc { get; set; }

    public Func<Pokemon, Pokemon, Move, bool> OnPreventMove { get; set; }
    public Func<Pokemon, Pokemon, bool> OnTrapped { get; set; }
    public Func<Pokemon, bool> OnBeforeMove { get; set; }
    public Func<Move, int> OnSetMove { get; set; }
    public Action<Pokemon> OnAfterMove { get; set; }
    public Action<Pokemon, Pokemon, Move> OnFaintedPokemon { get; set; }
    public Action<Pokemon, Pokemon> OnPokemonEnter { get; set; }
    public Action<Pokemon, Pokemon> OnAfterTurn { get; set; }
    public Action<Pokemon> OnOpponentStatus { get; set; }

    public Func<ConditionID, Pokemon, EffectData, bool> OnTrySetVolatile { get; set; }
    public Func<ConditionID, Pokemon, EffectData, bool> OnTrySetStatus { get; set; }

    public Func<Pokemon, int> OnEVGain { get; set; }
    public Func<float> OnExpGain { get; set; }
}
