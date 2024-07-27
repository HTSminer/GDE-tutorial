using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string moveName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] MoveCategory category;
    [SerializeField] CritBehavior critBehavior;

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

    public string Name => moveName;
    public string Description => description;

    public PokemonType Type { get => type; set => type = value; }
    public MoveCategory Category => category;
    public CritBehavior CritBehavior { get => critBehavior;  set => critBehavior = value; }
    public int Power => power;
    public int Accuracy { get => accuracy; set => accuracy = value; }
    public int PP => pp;
    public int MaxPP => maxPP;

    public bool AlwaysHits => alwaysHits;
    public int Priority => priority;

    public MoveEffects Effects => effects;
    public List<SecondaryEffects> Secondaries => secondaries;
    public RecoilMoveEffect Recoil => recoil;
    public HealType HealType => healType;
    public MoveTarget Target => target;
    public List<MoveFlag> Flags => flags;
    public AudioClip Sound => sound;

    public int GetHitCount()
    {
        if (hitRange == Vector2.zero)
                return 1;

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

    public List<StatBoost> Boosts => boosts;
    public ConditionID Status => status;
    public ConditionID VolatileStatus => volatileStatus;
    public ConditionID Weather => weather;
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance => chance;
    public MoveTarget Target => target;
}

[System.Serializable]
public class BreedingMoves
{
    [SerializeField] MoveBase move;
    [SerializeField] List<MoveParent> parent;
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
    public Stat Stat => stat;
    public int Boost => boost;
}

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