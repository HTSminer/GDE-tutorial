using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new berry item")]
public class BerryItem : ItemBase
{
    [Header("BerryTree Sprites")]
    [SerializeField] List<Sprite> plantSprites;
    [SerializeField] List<Sprite> sproutSprites;
    [SerializeField] List<Sprite> grownSprites;
    [SerializeField] List<Sprite> flowerSprites;
    [SerializeField] List<Sprite> berrySprites;

    [SerializeField] int moistureRate;

    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] int hpPercent;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Flavor")]
    [SerializeField] List<Flavor> flavors;

    [Header("Natural Gift")]
    [SerializeField] PokemonType giftType;
    [SerializeField] int giftPower;

    [Header("Fling")]
    [SerializeField] int flingPower;
    [SerializeField] MoveEffects flingEffect;

    public static BerryItem i { get; private set; }

    private void Awake() => i = this;

    public override bool Use(Pokemon pokemon)
    {
        // Restore HP
        if (restoreMaxHP || hpPercent > 0 || hpAmount > 0)
        {
            if (pokemon.HP == pokemon.MaxHp)
                return false;

            if (restoreMaxHP)
                pokemon.IncreaseHP(pokemon.MaxHp);
            else if (hpPercent > 0)
                pokemon.IncreaseHP(pokemon.MaxHp / hpPercent);
            else
                pokemon.IncreaseHP(hpAmount);
        }

        // Recover Status
        if (recoverAllStatus || status != ConditionID.none)
        {
            if (pokemon.Status == null && pokemon.VolatileStatus == null)
                return false;

            if (recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            }
            else
            {
                if (pokemon.Status.Id == status)
                    pokemon.CureStatus();
                else if (pokemon.VolatileStatus.Id == status)
                    pokemon.CureVolatileStatus();
                else
                    return false;
            }
        }

        // Restore PP
        if (restoreMaxPP)
            pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        else if (ppAmount > 0)
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));

        return true;
    }

    public List<Sprite> PlantSprites => plantSprites;
    public List<Sprite> SproutSprites => sproutSprites;
    public List<Sprite> GrownSprites => grownSprites;
    public List<Sprite> FlowerSprites => flowerSprites;
    public List<Sprite> BerrySprites => berrySprites;
}

[System.Serializable]
public struct Flavor
{
    public FlavorText flavorText;
    public int flavorLevel;
}

public enum FlavorText { Spicy, Dry, Sweet, Bitter, Sour, Smooth }