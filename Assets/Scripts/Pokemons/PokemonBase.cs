using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [Header("Pokedex Data")]
    [Header("------------------------------------------------------------")]
    [SerializeField] private int id;
    [SerializeField] private MrAmorphic.PokeApiPokemon pokeApiPokemon;
    [SerializeField] string speciesName;
    [SerializeField] string formName;
    [SerializeField] string speciesText;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] int height;
    [SerializeField] int weight;
    [SerializeField] bool isBaby;
    [SerializeField] bool isLegendary;
    [SerializeField] bool isMythical;
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    [field: Space(25)]
    [Header("Abilities")]
    [Header("------------------------------------------------------------")]
    [SerializeField] List<AbilityInfo> abilities;


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
    [SerializeField] List<EarnableEV> evYield;
    [SerializeField] int catchRate = 255;
    [SerializeField] int friendship = 70;
    [SerializeField] int expYield;
    [SerializeField] GrowthRateID growthRate;

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
    [SerializeField] List<MoveBase> learnableByBreeding;

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

    //public Dictionary<Stat, int> EvYields => new Dictionary<Stat, int>() 
    //{
    //    { Stat.HitPoints, hitPointsEvYield },
    //    { Stat.Attack, attackEvYield },
    //    { Stat.Defense, defenseEvYield },
    //    { Stat.Special_attack, spAttackEvYield },
    //    { Stat.Special_defense, spDefenseEvYield },
    //    { Stat.Speed, speedEvYield }
    //};

    public int GetExpForLevel(int level)
    {
        GrowthRate growth = GrowthRate.GetGrowthRate(growthRate);
        int expForLevel = growth.MinimumExpForLevel(level);
        return expForLevel;
    }


    public AbilityID GetRandomAbility()
    {
        var filteredAbilities = Abilities.Where(a => a.Ability > AbilityID.none).Select(a => a.Ability).ToList();
        if (filteredAbilities.Count == 0)
        {
            throw new InvalidOperationException("No abilities available");
        }

        int r = UnityEngine.Random.Range(0, filteredAbilities.Count);
        return filteredAbilities[r];
    }


    public int Id { get => id; set => id = value; }
    public MrAmorphic.PokeApiPokemon PokeApiPokemon { get => pokeApiPokemon; set => pokeApiPokemon = value; }
    public string Name { get => speciesName; set => speciesName = value; }
    public string FormName => formName;
    public string Species => speciesText;
    public int Height { get => height; set => height = value; }
    public int Weight { get => weight; set => weight = value; }
    public bool IsBaby { get => isBaby; set => isBaby = value; }
    public bool IsLegendary { get => isLegendary; set => isLegendary = value; }
    public bool IsMythical { get => isMythical; set => isMythical = value; }
    public List<AbilityInfo> Abilities { get => abilities; set => abilities = value; }
    public PokemonType Type1 { get => type1; set => type1 = value; }
    public PokemonType Type2 { get => type2; set => type2 = value; }
    public string Description { get => description; set => description = value; }

    public Sprite FrontSprite { get => frontSprite; set => frontSprite = value; }
    public Sprite BackSprite { get => backSprite; set => backSprite = value; }

    public int MaxHp { get => maxHp; set => maxHp = value; }
    public int Attack { get => attack; set => attack = value; }
    public int Defense { get => defense; set => defense = value; }
    public int SpAttack { get => spAttack; set => spAttack = value; }
    public int SpDefense { get => spDefense; set => spDefense = value; }
    public int Speed { get => speed; set => speed = value; }

    public List<EarnableEV> EvYield { get => evYield; set => evYield = value; }

    //public int HP_EV { get => hitPointsEvYield; set => hitPointsEvYield = value; }
    //public int ATK_EV { get => attackEvYield; set => attackEvYield = value; }
    //public int DEF_EV { get => defenseEvYield; set => defenseEvYield = value; }
    //public int SPA_EV { get => spAttackEvYield; set => spAttackEvYield = value; }
    //public int SPD_EV { get => spDefenseEvYield; set => spDefenseEvYield = value; }
    //public int SPE_EV { get => speedEvYield; set => speedEvYield = value; }

    public int CatchRate { get => catchRate; set => catchRate = value; }
    public int Friendship { get => friendship; set => friendship = value; }
    public int ExpYield { get => expYield; set => expYield = value; }

    public GrowthRateID GrowthRateId { get => growthRate; set => growthRate = value; }
    public GrowthRate GrowthRate => GrowthRate.GetGrowthRate(growthRate);

    public List<LearnableMoves> LearnableMoves { get => learnableMoves; set => learnableMoves = value; }
    public List<MoveBase> LearnableByItems { get => learnableByItems; set => learnableByItems = value; }
    public List<MoveBase> LearnableByBreeding {
        get => learnableByBreeding; set => learnableByBreeding = value;
    }

    public List<Forms> Forms {
        get => forms; set => forms = value;
    }
    public List<Evolution> Evolutions {
        get => evolutions; set => evolutions = value;
    }
    public List<EggGroups> EggGroup {
        get => eggGroup; set => eggGroup = value;
    }
    public int EggCycles { get => eggCycles; set => eggCycles = value; }
    public PokemonGenderRatio GenderRatio { get => genderRatio; set => genderRatio = value; }

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

    public ItemBase CItem { get => cItem; set => cItem = value; }
    public ItemBase RItem { get => rItem; set => rItem = value; }
    public ItemBase SItem { get => sItem; set => sItem = value; }
}

[System.Serializable]
public class AbilityInfo
{
    public AbilityID ability;
    public bool isHidden;

    public AbilityID Ability { get => ability; set => ability = value; }
    public bool IsHidden { get => isHidden; set => isHidden = value; }
}

[System.Serializable]
public class LearnableMoves
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base { get => moveBase; set => moveBase = value; }

    public int Level { get => level; set => level = value; }
}

public enum Weather { None, Sunny, Rain, Sandstorm }

[System.Serializable]
public class EarnableEV
{
    [SerializeField] Stat statAttribute;
    [Range(1, 3)]
    [SerializeField] int statValue;

    public Stat StatAttribute => statAttribute;
    public int StatValue => statValue;

    public EarnableEV(Stat stat, int value)
    {
        statAttribute = stat;
        statValue = value;
    }
}

[System.Serializable]
public class Evolution
{
    [SerializeField] PokemonBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;
    [SerializeField] EvolutionItem heldItem;
    [SerializeField] PokemonBase requiredPokemon;
    [SerializeField] TimeOfDay timeOfDay;
    [SerializeField] Places location;
    [SerializeField] Weather weather;
    [SerializeField] PokemonGender gender;

    public PokemonBase EvolvesInto { get => evolvesInto; set => evolvesInto = value; }
    public int RequiredLevel { get => requiredLevel; set => requiredLevel = value; }
    public EvolutionItem RequiredItem { get => requiredItem; set => requiredItem = value; }
    public EvolutionItem HeldItem { get => heldItem; set => heldItem = value; }
    public PokemonBase RequiredPokemon { get => requiredPokemon; set => requiredPokemon = value; }
    public TimeOfDay TimeOfDay { get => timeOfDay; set => timeOfDay = value; }
    public Places Location { get => location; set => location = value; }
    public Weather Weather { get => weather; set => weather = value; }
    public PokemonGender Gender { get => gender; set => gender = value; }
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

public enum Stat { Attack, Defense, Special_attack, Special_defense, Speed, Accuracy, Evasion, HitPoints }

public enum PokemonGenderRatio 
{ 
    OneInTwoFemale, OneInFourFemale, OneInEightFemale, ThreeInFourFemale,
    SevenInEightFemale, FemaleOnly, MaleOnly, Genderless, Ditto 
}

public enum PokemonGender { NotSet, Female, Male, Genderless, Ditto }

public enum EggGroups 
{ 
    no_eggs, monster, humanshape, water1, water2, water3, bug, mineral, flying,
    fairy, ditto, grass, dragon, ground, unknown, plant, indeterminate
}

public enum TimeOfDay { None, Morning, Day, Night }

public enum Places { none, Grassy_Rock }

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
