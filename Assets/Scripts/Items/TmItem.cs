using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Pokemon pokemon)
    {
        // Learning move is handled from Inventory UI, if it was learned then return true
        return pokemon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(move);
    }

    public override bool IsReusable => isHM;

    public MoveBase Move { get => move; set => move = value; }
    public bool IsHM { get => isHM; set => isHM = value; }
    
}
