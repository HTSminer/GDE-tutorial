using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public string EffectMessage { get; set; }

    public Action<Pokemon, Pokemon> OnStart { get; set; }
    public Action<Pokemon, Pokemon> OnAfterTurn { get; set; }
    public Action<Pokemon> OnAfterMove { get; set; }
    public Func<Pokemon, bool> OnBeforeMove { get; set; }

    public Action<Pokemon, Pokemon> OnWeather { get; set; }
    public Func<Pokemon, Pokemon, Move, float> OnDamageModify { get; set; }
}
