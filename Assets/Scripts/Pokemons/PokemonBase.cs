using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [Header("Pokedex Data")]
    [Header("------------------------------------------------------------")]
    [SerializeField] int index;
    [SerializeField] string speciesName;
    [SerializeField] string formName;
    [SerializeField] string speciesText;
    [SerializeField] float height;
    [SerializeField] float weight;

    [SerializeField] List<AbilityID> abilities;
    [SerializeField] List<AbilityID> hAbilities;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    [TextArea]
    [SerializeField] string description;

    [field: Space(50)]
    [Header("Forms")]
    [Header("------------------------------------------------------------")]
    [SerializeField] List<Forms> forms;

    [field: Space(50)]
    [Header("Sprites")]
    [Header("------------------------------------------------------------")]
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] Sprite frontSpriteShiny;
    [SerializeField] Sprite backSpriteShiny;
    [SerializeField] List<Sprite> menuSprite;
    [SerializeField] List<Sprite> menuSpriteShiny;
    [SerializeField] List<Sprite> downSprite;
    [SerializeField] List<Sprite> upSprite;
    [SerializeField] List<Sprite> leftSprite;
    [SerializeField] List<Sprite> rightSprite;

    [field: Space(50)]
    [Header("Base Stats")]
    [Header("------------------------------------------------------------")]
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [field: Space(50)]
    [Header("Training")]
    [Header("------------------------------------------------------------")]
    [SerializeField] int hitPointsEvYield = 0;
    [SerializeField] int attackEvYield = 0;
    [SerializeField] int defenseEvYield = 0;
    [SerializeField] int spAttackEvYield = 0;
    [SerializeField] int spDefenseEvYield = 0;
    [SerializeField] int speedEvYield = 0;
    [SerializeField] int catchRate = 255;
    [SerializeField] int friendship = 70;
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [field: Space(50)]
    [Header("Breeding")]
    [Header("------------------------------------------------------------")]
    [SerializeField] List<EggGroups> eggGroup;
    [SerializeField] PokemonGenderRatio genderRatio;
    [SerializeField] int eggCycles;

    [field: Space(50)]
    [Header("Moves")]
    [Header("------------------------------------------------------------")]
    [SerializeField] List<LearnableMoves> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;
    [SerializeField] List<BreedingMoves> learnableByBreeding;

    [field: Space(50)]
    [Header("Evolutions")]
    [Header("------------------------------------------------------------")]
    [SerializeField] List<Evolution> evolutions;

    [field: Space(50)]
    [Header("Wild held Items")]
    [Header("------------------------------------------------------------")]
    [SerializeField] ItemBase cItem;
    [SerializeField] ItemBase rItem;
    [SerializeField] ItemBase sItem;

    public static int MaxNumberOfMoves { get; set; } = 4;

    public Dictionary<Stat, int> EvYields => new Dictionary<Stat, int>() 
    {
        { Stat.HitPoints, hitPointsEvYield },
        { Stat.Attack, attackEvYield },
        { Stat.Defense, defenseEvYield },
        { Stat.SpAttack, spAttackEvYield },
        { Stat.SpDefense, spDefenseEvYield },
        { Stat.Speed, speedEvYield }
    };

    public int GetExpForLevel(int level)
    {
        if (level <= 1)
            return 0;

        if (growthRate == GrowthRate.Erratic)
        {
            if (level < 50)
                return Mathf.FloorToInt((Mathf.Pow(level, 3f) * (100f - level)) / 50f);
            else if (level >= 50 && level < 68)
                return Mathf.FloorToInt((Mathf.Pow(level, 3f) * (150f - level)) / 100f);
            else if (level >= 68 && level < 98)
                return Mathf.FloorToInt((Mathf.Pow(level, 3f) * ((1911f - (10f * level)) / 3f)) / 500f);
            else
                return Mathf.FloorToInt((Mathf.Pow(level, 3f) * (160f - level)) / 100f);
        }
        else if (growthRate == GrowthRate.Fast)
            return Mathf.FloorToInt(4 * (Mathf.Pow(level, 3f)) / 5f);
        else if (growthRate == GrowthRate.MediumFast)
            return Mathf.FloorToInt(Mathf.Pow(level, 3f));
        else if (growthRate == GrowthRate.MediumSlow)
            return Mathf.FloorToInt((6 * (Mathf.Pow(level, 3f)) / 5f) - 15f * (Mathf.Pow(level, 2f)) + 100f * level - 140f);
        else if (growthRate == GrowthRate.Slow)
            return Mathf.FloorToInt(5 * (Mathf.Pow(level, 3f)) / 4f);
        else if (growthRate == GrowthRate.Fluctuating)
        {
            if (level < 15)
                return Mathf.FloorToInt(Mathf.Pow(level, 3f) * ((((level + 1f) / 3f) + 24f) / 50f)); //125 * (26 / 50)
            else if (level >= 15 && level < 36)
                return Mathf.FloorToInt(Mathf.Pow(level, 3f) * ((level + 14f) / 50f));
            else
                return Mathf.FloorToInt(Mathf.Pow(level, 3f) * (((level / 2f) + 32f) / 50f));
        }

        return -1;
    }

    public AbilityID GetRandomAbility()
    {
        var abilities = Abilities.Where(a => a > 0).ToList();

        int r = Random.Range(0, abilities.Count);
        
        return Abilities[r];
    }


    public int Index => index;
    public string Name => speciesName;
    public string FormName => formName;
    public string Species => speciesText;
    public float Height => height;
    public float Weight => weight;
    public List<AbilityID> Abilities => abilities;
    public List<AbilityID> HAbilities => hAbilities;
    public PokemonType Type1
    {
        get => type1;
        set => type1 = value;
    }
    public PokemonType Type2
    {
        get => type2;
        set => type2 = value;
    }
    public string Description => description;

    public Sprite FrontSprite { get => frontSprite; set => frontSprite = value; }
    public Sprite BackSprite => backSprite;

    public int MaxHp => maxHp;
    public int Attack => attack;
    public int Defense => defense;
    public int SpAttack => spAttack;
    public int SpDefense => spDefense;
    public int Speed => speed;

    public int CatchRate => catchRate;
    public int Friendship => friendship;
    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;

    public List<LearnableMoves> LearnableMoves => learnableMoves;
    public List<MoveBase> LearnableByItems => learnableByItems;
    public List<BreedingMoves> LearnableByBreeding => learnableByBreeding;

    public List<Forms> Forms => forms;
    public List<Evolution> Evolutions => evolutions;
    public List<EggGroups> EggGroup => eggGroup;
    public int EggCycles => eggCycles;
    public PokemonGenderRatio GenderRatio => genderRatio;

    public List<Sprite> WalkDownSprites
    {
        get => downSprite; set => downSprite = value;
    }

    public List<Sprite> WalkUpSprites
    {
        get => upSprite; set => upSprite = value;
    }

    public List<Sprite> WalkLeftSprites
    {
        get => leftSprite; set => leftSprite = value;
    }

    public List<Sprite> WalkRightSprites
    {
        get => rightSprite; set => rightSprite = value;
    }

    public ItemBase CItem => cItem;
    public ItemBase RItem => rItem;
    public ItemBase SItem => sItem;
}

[System.Serializable]
public class LearnableMoves
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

[System.Serializable]
public class MoveParent
{
    [SerializeField] PokemonBase parent;
}

public enum Weather { Sunny, Rain, Sandstorm }

[System.Serializable]
public class Evolution
{
    [SerializeField] PokemonBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;
    [SerializeField] EvolutionItem heldItem;
    [SerializeField] TimeOfDay timeOfDay;
    [SerializeField] Places location;
    [SerializeField] Weather weather;
    [SerializeField] Gender gender;

    public PokemonBase EvolvesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
    public EvolutionItem RequiredItem => requiredItem;
    public EvolutionItem HeldItem => heldItem;
    public TimeOfDay TimeOfDay => timeOfDay;
    public Places Location => location;
    public Weather Weather => weather;
    public Gender Gender => gender;
}

[System.Serializable]
public class Forms
{
    [SerializeField] PokemonBase pokemonForm;
    [SerializeField] EvolutionItem heldItem;

    public PokemonBase PokemonForm => pokemonForm;
    public EvolutionItem HeldItem => heldItem;
}

public enum PokemonType 
{ 
    None, Normal, Fire, Water, Grass, Electric, Ice, Fighting, Poison, 
    Ground, Flying, Psychic, Bug, Rock, Ghost, Dragon, Dark, Steel, Fairy 
}

public enum GrowthRate { Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating }

public enum Stat { Attack, Defense, SpAttack, SpDefense, Speed, Accuracy, Evasion, HitPoints }

public enum PokemonGenderRatio 
{ 
    OneInTwoFemale, OneInFourFemale, OneInEightFemale, ThreeInFourFemale,
    SevenInEightFemale, FemaleOnly, MaleOnly, Genderless, Ditto
}

public enum PokemonGender { NotSet, Female, Male, Genderless, Ditto }

public enum Gender { none, Male, Female, Ditto }

public enum EggGroups 
{ 
    none, Monster, HumanLike, Water1, Water2, Water3, Bug, Mineral, Flying, Amorphous,
    Field, Fairy, Ditto, Grass, Dragon, Unknown
}

public enum TimeOfDay { None, Morning, Day, Night }

public enum Places { none, Grassy_Rock }

[System.Serializable]
public class EvYield
{
    [SerializeField] Stat stat;
    [SerializeField] int yield;

    public Stat Stat => stat;
    public int Yield => yield;
}

public class TypeChart
{
    static float[][] chart =
    {
        //                      NOR  FIR  WAT  GRA  ELE  ICE  FGH  PSN  GRN  FLY  PSY  BUG  RCK  GHO  DRG  DRK  STL  FAI
        /*NOR*/   new float[] { 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f, .5f,  0f,  1f,  1f, .5f,  1f  },
        /*FIR*/   new float[] { 1f, .5f, .5f,  2f,  1f,  2f,  1f,  1f,  1f,  1f,  1f,  2f, .5f,  1f, .5f,  1f,  2f,  1f  },
        /*WAT*/   new float[] { 1f,  2f, .5f, .5f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  2f,  1f, .5f,  1f,  1f,  1f  },
        /*GRA*/   new float[] { 1f, .5f,  2f, .5f,  1f,  1f,  1f, .5f,  2f, .5f,  1f, .5f,  2f,  1f, .5f,  1f, .5f,  1f  },
        /*ELE*/   new float[] { 1f,  1f,  2f, .5f, .5f,  1f,  1f,  1f,  0f,  2f,  1f,  1f,  1f,  1f, .5f,  1f,  1f,  1f  },
        /*ICE*/   new float[] { 1f, .5f, .5f,  2f,  1f, .5f,  1f,  1f,  2f,  2f,  1f,  1f,  1f,  1f,  2f,  1f, .5f,  1f  },
        /*FGH*/   new float[] { 2f,  1f,  1f,  1f,  1f,  2f,  1f, .5f,  1f, .5f, .5f, .5f,  2f,  0f,  1f,  2f,  2f, .5f  },
        /*PSN*/   new float[] { 1f,  1f,  1f,  2f,  1f,  1f,  1f, .5f, .5f,  1f,  1f,  1f, .5f, .5f,  1f,  1f,  0f,  2f  },
        /*GRN*/   new float[] { 1f,  2f,  1f, .5f,  2f,  1f,  1f,  2f,  1f,  0f,  1f, .5f,  2f,  1f,  1f,  1f,  2f,  1f  },
        /*FLY*/   new float[] { 1f,  1f,  1f,  2f, .5f,  1f,  2f,  1f,  1f,  1f,  1f,  2f, .5f,  1f,  1f,  1f, .5f,  1f  },
        /*PSY*/   new float[] { 1f,  1f,  1f,  1f,  1f,  1f,  2f,  2f,  1f,  1f, .5f,  1f,  1f,  1f,  1f,  0f, .5f,  1f  },
        /*BUG*/   new float[] { 1f, .5f,  1f,  2f,  1f,  1f, .5f, .5f,  1f, .5f,  2f,  1f,  1f, .5f,  1f,  2f, .5f, .5f  },
        /*RCK*/   new float[] { 1f,  2f,  1f,  1f,  1f,  2f, .5f,  1f, .5f,  2f,  1f,  2f,  1f,  1f,  1f,  1f, .5f,  1f  },
        /*GHO*/   new float[] { 0f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f, .5f,  1f,  1f  },
        /*DRG*/   new float[] { 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f, .5f,  0f  },
        /*DRK*/   new float[] { 1f,  1f,  1f,  1f,  1f,  1f, .5f,  1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f, .5f,  1f, .5f  },
        /*STL*/   new float[] { 1f, .5f, .5f,  1f, .5f,  2f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  1f, .5f,  2f  },
        /*FAI*/   new float[] { 1f, .5f,  1f,  1f,  1f,  1f,  2f, .5f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  2f, .5f,  1f  }
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
