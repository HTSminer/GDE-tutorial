using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new battle item")]
public class BattleItem : ItemBase
{
    [SerializeField] bool isEscapeItem;

    BattleSystem bs;

    public static BattleItem i { get; private set; }

    private void Awake() => i = this;

    public override bool Use(Pokemon pokemon)
    {
        if (isEscapeItem)
            return true;

        return false;
    }

    public bool IsEscapeItem => isEscapeItem;
}
