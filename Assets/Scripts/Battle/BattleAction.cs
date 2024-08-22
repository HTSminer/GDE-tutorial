using UnityEngine;

public enum ActionType { Move, SwitchPokemon, MegaEvolve, UseItem, Run }

public class BattleAction 
{
    public ActionType Type { get; set; }
    public BattleUnit User { get; set; }
    public BattleUnit Target { get; set; }

    public Move Move { get; set; } // For performing Moves
    public Pokemon SelectedPokemon { get; set; } // For switching Pokemon

    public bool IsInvalid { get; set; }

    public int Priority => (Type == ActionType.Move) ? Move.Base.Priority : 99;
}
