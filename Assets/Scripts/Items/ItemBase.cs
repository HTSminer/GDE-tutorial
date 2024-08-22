using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private MrAmorphic.PokeApiItem pokeApiItem;
    
    [SerializeField] string itemName;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;

    public int Id { get => id; set => id = value; }
    public MrAmorphic.PokeApiItem PokeApiItem { get => pokeApiItem; set => pokeApiItem = value; }
    public HeldItemID HeldItemId;
    public virtual string Name { get => itemName; set => itemName = value; }
    public string Description {
        get => description; set => description = value;
    }
    public Sprite Icon {
        get => icon; set => icon = value;
    }
    public float Cost {
        get => price; set => price = value;
    }
    public bool IsSellable {
        get => isSellable; set => isSellable = value;
    }

    public virtual bool Use(Pokemon pokemon) => false;

    public virtual float ReduceDamageByType(MoveBase move) => 1f;

    public virtual bool IsReusable => false;
    public bool CanUseInBattle { get; set; } = true;
    public bool CanUseOutsideBattle { get; set; } = true;
}
