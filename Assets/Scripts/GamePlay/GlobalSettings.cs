using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] Gradient healthBarGradient;
    [Space(50)]
    [Header("Pok�mon")]
    [SerializeField] int maxLevel = 100;
    [SerializeField] int maxEvs = 510;
    [SerializeField] int maxEvPerStat = 252;

    public Color HighlightedColor => highlightedColor;
    public Gradient HealthBarGradient => healthBarGradient;
    public int MaxLevel => maxLevel;
    public int MaxEvs => maxEvs;
    public int MaxEvPerStat => maxEvPerStat;

    public static GlobalSettings i { get; private set; }
    private void Awake() => i = this;
}
