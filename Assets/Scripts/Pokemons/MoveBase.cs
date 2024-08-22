using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private MrAmorphic.PokeApiMove pokeApiMove;
    [SerializeField] string moveName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] MoveCategory category;
    [SerializeField] CritBehavior critBehavior;
    [SerializeField] PowerBasedOn powerBasedOn = PowerBasedOn.value;

    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] int maxPP;

    [SerializeField] bool alwaysHits;
    [SerializeField] int priority;

    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] RecoilMoveEffect recoil = new RecoilMoveEffect();
    [SerializeField] HealType healType;

    [SerializeField] MoveTarget target;

    [SerializeField] Vector2Int hitRange;
    [SerializeField] List<MoveFlag> flags;

    [SerializeField] AudioClip sound;

    public int Id { get => id; set => id = value; }
    public MrAmorphic.PokeApiMove PokeApiMove { get => pokeApiMove; set => pokeApiMove = value; }
    public string Name { get => moveName; set => moveName = value; }
    public string Description { get => description; set => description = value; }

    public PokemonType Type { get => type; set => type = value; }
    public MoveCategory Category { get => category; set => category = value; }
    public CritBehavior CritBehavior { get => critBehavior; set => critBehavior = value; }
    public PowerBasedOn PowerBasedOn { get => powerBasedOn; set => powerBasedOn = value; }
    public int Power { get => power; set => power = value; }
    public int Accuracy { get => accuracy; set => accuracy = value; }
    public int PP { get => pp; set => pp = value; }
    public int MaxPP { get => maxPP; set => maxPP = value; }

    public bool AlwaysHits {
        get => alwaysHits; set => alwaysHits = value;
    }
    public int Priority {
        get => priority; set => priority = value;
    }

    public MoveEffects Effects {
        get => effects; set => effects = value;
    }
    public List<SecondaryEffects> Secondaries {
        get => secondaries; set => secondaries = value;
    }
    public RecoilMoveEffect Recoil {
        get => recoil; set => recoil = value;
    }
    public HealType HealType {
        get => healType; set => healType = value;
    }
    public MoveTarget Target {
        get => target; set => target = value;
    }
    public List<MoveFlag> Flags {
        get => flags; set => flags = value;
    }
    public AudioClip Sound {
        get => sound; set => sound = value;
    }

    public int GetHitCount()
    {
        if (hitRange == Vector2.zero) return 1;

        int hitCount = 1;
        if (hitRange.y == 0)
            hitCount = hitRange.x;
        else
            hitCount = Random.Range(hitRange.x, hitRange.y + 1);

        return hitCount;
    }

    public bool HasFlag(MoveFlag flag)
    {
        if (flags != null && flags.Contains(flag))
            return true;

        return false;
    }
}

public class EffectData
{
    public EffectSource Source { get; set; }
    public int SourceId { get; set; }
}

[System.Serializable]
public class RecoilMoveEffect
{
    public RecoilType recoilType;
    public int recoilDamage = 0;
}

[System.Serializable]
public class MoveEffects : EffectData
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] ConditionID weather;

    public List<StatBoost> Boosts {
        get => boosts; set => boosts = value;
    }
    public ConditionID Status {
        get => status; set => status = value;
    }
    public ConditionID VolatileStatus {
        get => volatileStatus; set => volatileStatus = value;
    }
    public ConditionID Weather {
        get => weather; set => weather = value;
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance {
        get => chance; set => chance = value;
    }
    public MoveTarget Target {
        get => target; set => target = value;
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
    public Stat Stat {
        get => stat; set => stat = value;
    }
    public int Boost {
        get => boost; set => boost = value;
    }
}

public enum PowerBasedOn { value, targetWeight, weightDiff, positiveStatusCount, consecutiveMoveUse }

public enum MoveCategory { Physical, Special, Status }

public enum MoveTarget { Foe, Self }

public enum CritBehavior { none, HighCritRatio, AlwaysCrits, NeverCrits }

public enum RecoilType { none, RecoilByMaxHP, RecoilByCurrentHP, RecoilByDamage }

public enum HealType { None, Full, Half }

public enum EffectSource { Ability, Item, Move, Condition }

public enum MoveFlag 
{ 
    Contact, Punch, Bite, Pulse, Sound, InstantKO, Binding, Bullet, Bomb, FirstTurn, StatusMultiplier,
    ConsecutiveMoveUsedMultiplier, ProtectBlocked, Charge, Recharge, Reflect, Snatch, MirrorCopy, Gravity, Defrost,
    HealBlock, IgnoreSubstitute, Powder, Jaw, Mental, Dance
}