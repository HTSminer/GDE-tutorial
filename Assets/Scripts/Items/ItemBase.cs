using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string itemName;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;

    public HeldItemID HeldItemId;
    public virtual string Name => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public float Price => price;
    public bool IsSellable => isSellable;

    public virtual bool Use(Pokemon pokemon) => false;

    public virtual float ReduceDamageByType(MoveBase move) => 1f;

    public virtual bool IsReusable => false;
    public virtual bool CanUseInBattle => true;
    public virtual bool CanUseOutsideBattle => true;
}
