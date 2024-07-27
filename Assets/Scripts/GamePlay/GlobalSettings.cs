using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] Gradient healthBarGradient;
    [Space(50)]
    [Header("Pokémon")]
    [SerializeField] int maxEvs = 510;
    [SerializeField] int maxEvPerStat = 252;

    public int MaxEvs => maxEvs;
    public int MaxEvPerStat => maxEvPerStat;
    public Color HighlightedColor => highlightedColor;
    public Gradient HealthBarGradient => healthBarGradient;

    public static GlobalSettings i { get; private set; }
    private void Awake()
    {
        i = this;
    }
}
