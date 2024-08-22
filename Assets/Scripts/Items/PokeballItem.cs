using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new pokeball item")]
public class PokeballItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;

    public override bool Use(Pokemon pokemon)
    {
        return true;
    }

    public float CatchRateModifier => catchRateModifier;
}
