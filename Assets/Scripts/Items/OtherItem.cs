using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new item")]
public class OtherItem : ItemBase
{
    BattleDialog dialogBox;

    public static OtherItem i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}

