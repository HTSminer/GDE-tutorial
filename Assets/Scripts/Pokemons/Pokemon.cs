using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] PokemonGender gender;

    [SerializeField] string trainer;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    #region Variables
    public PokemonBase Base => _base;
    public int Level => level;
    public PokemonGender Gender => gender;
    public string Trainer { get => trainer; set => trainer = value; }

    public int Exp { get; set; }
    public int HP { get; set; }
    public bool IsEgg { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatEffortValues { get; private set; }
    public Dictionary<Stat, int> StatIndividualValues { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; }

    public List<Move> Moves { get; set; }
    public bool FirstTurn { get; set; }
    public Move CurrentMove { get; set; }
    public Move PrevMoveUsed { get; set; }
    public bool MovesFirst { get; set; } = false;
    public bool CritTaken { get; set; } = false;
    public bool hasLeveled { get; set; } = false;

    public bool isMega { get; set; } = false;
    public bool canMega { get; set; }
    public Pokemon PokemonBeforeMega { get; set; }

    public string Nature { get; set; }

    public Ability Ability { get; set; }

    public HeldItems HeldItems { get; set; }
    public ItemBase HeldItem;
    public bool HoldingItem { get; set; } = false;

    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public int SameMoveUsed { get; set; } = 1;

    #endregion

    public void Init()
    {
        //Generate Moves
        Moves = new List<Move>();

        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumberOfMoves)
                break;
        }

        StatIndividualValues = new Dictionary<Stat, int>()
            {
                { Stat.HitPoints, 0 },
                { Stat.Attack, 0 },
                { Stat.Defense, 0 },
                { Stat.Special_attack, 0 },
                { Stat.Special_defense, 0 },
                { Stat.Speed, 0 }
            };

        StatEffortValues = new Dictionary<Stat, int>()
        {
            { Stat.HitPoints, 0 },
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.Special_attack, 0 },
            { Stat.Special_defense, 0 },
            { Stat.Speed, 0 }
        };

        Nature = Natures.RandomNature().GetName();
        DecideGender();
        RandomIVs();

        SetHeldItem();

        Exp = Base.GetExpForLevel(level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();

        Ability = AbilityDB.Abilities[Base.GetRandomAbility()];

        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        StatIndividualValues = new Dictionary<Stat, int>()
        {
            { Stat.HitPoints, saveData.IV_HP },
            { Stat.Attack, saveData.IV_ATK },
            { Stat.Defense, saveData.IV_DEF },
            { Stat.Special_attack, saveData.IV_SPA },
            { Stat.Special_defense, saveData.IV_SPD },
            { Stat.Speed, saveData.IV_SPE }
        };
        StatEffortValues = new Dictionary<Stat, int>()
        {
            { Stat.HitPoints, saveData.EV_HP },
            { Stat.Attack, saveData.EV_ATK },
            { Stat.Defense, saveData.EV_DEF },
            { Stat.Special_attack, saveData.EV_SPA },
            { Stat.Special_defense, saveData.EV_SPD },
            { Stat.Speed, saveData.EV_SPE }
        };
        Nature = saveData.nature;

        HeldItem = (saveData.heldItem == null) ? null : ItemDB.GetObjectByName(saveData.heldItem);
        HeldItems = HeldItemsDB.HeldItems[saveData.itemId];
        
        gender = saveData.gender;
        Exp = saveData.exp;

        Status = (saveData.statusId != null) ? ConditionsDB.Conditions[saveData.statusId.Value] : null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();
        Ability.Id = saveData.ability;

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            IV_HP = StatIndividualValues[Stat.HitPoints],
            IV_ATK = StatIndividualValues[Stat.Attack],
            IV_DEF = StatIndividualValues[Stat.Defense],
            IV_SPA = StatIndividualValues[Stat.Special_attack],
            IV_SPD = StatIndividualValues[Stat.Special_defense],
            IV_SPE = StatIndividualValues[Stat.Speed],
            EV_HP = StatEffortValues[Stat.HitPoints],
            EV_ATK = StatEffortValues[Stat.Attack],
            EV_DEF = StatEffortValues[Stat.Defense],
            EV_SPA = StatEffortValues[Stat.Special_attack],
            EV_SPD = StatEffortValues[Stat.Special_defense],
            EV_SPE = StatEffortValues[Stat.Speed],
            nature = Nature,
            heldItem = HeldItem != null ? HeldItem.name : null,
            itemId = HeldItem != null ? HeldItem.HeldItemId : HeldItemID.none,
            gender = Gender,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),
            ability = Ability.Id
        };

        return saveData;
    }

    public void SetOwner(PlayerController player) => trainer = player.Name;

    public void DecideGender()
    {
        if (gender != PokemonGender.NotSet) return;

        int ran = Random.Range(1, 8);
        switch (Base.GenderRatio)
        {
            case PokemonGenderRatio.OneInTwoFemale:
                if (ran <= 4)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.OneInFourFemale:
                if (ran <= 2)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.OneInEightFemale:
                if (ran <= 1)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.ThreeInFourFemale:
                if (ran <= 6)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.SevenInEightFemale:
                if (ran <= 7)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.FemaleOnly:
                gender = PokemonGender.Female;
                break;
            case PokemonGenderRatio.MaleOnly:
                gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.Ditto:
                gender = PokemonGender.Ditto;
                break;
            default:
                gender = PokemonGender.Genderless;
                break;
        }
    }

    public void GainEvs(Dictionary<Stat, int> evGained)
    {
        HeldItems?.OnEVGain?.Invoke(this);

        int machoBrace = 1;
        if (HeldItem != null)
            machoBrace = HeldItem.Name == "Macho Brace" ? 2 : 1;

        // sev stands for "Stat Effort Values"
        foreach (var sev in StatEffortValues.ToArray())
        {
            if (sev.Value < GlobalSettings.i.MaxEvPerStat && GetTotalEvs() < GlobalSettings.i.MaxEvs)
            {
                evGained[sev.Key] = Mathf.Clamp(evGained[sev.Key], 0, (GlobalSettings.i.MaxEvs - GetTotalEvs()));
                StatEffortValues[sev.Key] = Mathf.Clamp((StatEffortValues[sev.Key] += (evGained[sev.Key] * machoBrace)), 0, GlobalSettings.i.MaxEvPerStat);
            }
        }
    }

    public void RandomIVs()
    {
        foreach (var iv in StatIndividualValues.ToArray())
            StatIndividualValues[iv.Key] = Random.Range(1, 32);
    }

    public int GetTotalEvs() => StatEffortValues.Values.Sum();

    public void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt(Mathf.FloorToInt((((2 * Base.Attack) + StatIndividualValues[Stat.Attack] + (StatEffortValues[Stat.Attack] / 4)) * level) / 100 + 5) * Natures.GetNature(Nature).GetATK()));
        Stats.Add(Stat.Defense, Mathf.FloorToInt(Mathf.FloorToInt((((2 * Base.Defense) + StatIndividualValues[Stat.Defense] + (StatEffortValues[Stat.Defense] / 4)) * level) / 100 + 5) * Natures.GetNature(Nature).GetDEF()));
        Stats.Add(Stat.Special_attack, Mathf.FloorToInt(Mathf.FloorToInt((((2 * Base.SpAttack) + StatIndividualValues[Stat.Special_attack] + (StatEffortValues[Stat.Special_attack] / 4)) * level) / 100 + 5) * Natures.GetNature(Nature).GetSPA()));
        Stats.Add(Stat.Special_defense, Mathf.FloorToInt(Mathf.FloorToInt((((2 * Base.SpDefense) + StatIndividualValues[Stat.Special_defense] + (StatEffortValues[Stat.Special_defense] / 4)) * level) / 100 + 5) * Natures.GetNature(Nature).GetSPD()));
        Stats.Add(Stat.Speed, Mathf.FloorToInt(Mathf.FloorToInt((((2 * Base.Speed) + StatIndividualValues[Stat.Speed] + (StatEffortValues[Stat.Speed] / 4)) * level) / 100 + 5) * Natures.GetNature(Nature).GetSPE()));

        int oldMaxHP = MaxHp;
        MaxHp = Mathf.FloorToInt((2 * Base.MaxHp + StatIndividualValues[Stat.HitPoints] + (StatEffortValues[Stat.HitPoints] / 4f)) * Level) / 100 + Level + 10;

        if (oldMaxHP != 0)
            HP += MaxHp - oldMaxHP;
    }

    private void SetHeldItem()
    {
        int r = Random.Range(1, 101);

        if (_base.CItem != null)
        {
            HoldingItem = true;
            HeldItem = _base.CItem;
            HeldItems = HeldItemsDB.HeldItems[HeldItem.HeldItemId];
        }
        else if (_base.RItem != null && r >= 50)
        {
            HoldingItem = true;
            HeldItem = _base.RItem;
            HeldItems = HeldItemsDB.HeldItems[HeldItem.HeldItemId];
        }
        else if (_base.SItem != null && r >= 95)
        {
            HoldingItem = true;
            HeldItem = _base.SItem;
            HeldItems = HeldItemsDB.HeldItems[HeldItem.HeldItemId];
        }
        else if (HeldItem != null)
        {
            HoldingItem = true;
            HeldItems = HeldItemsDB.HeldItems[HeldItem.HeldItemId];
        }
        else
        {
            HoldingItem = true;
            HeldItem = null;
            HeldItems = HeldItemsDB.HeldItems[HeldItemID.none];
        }
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.Special_attack, 0 },
            { Stat.Special_defense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasion, 0 }
        };
    }

    int GetStat(Stat stat)
    {
        float statItem = 1f;
        if (HeldItem != null)
        {
            if (HeldItem.Name == "Choice Band")
                statItem = stat == Stat.Attack ? 1.5f : 1f;
            else if (HeldItem.Name == "Choice Specs")
                statItem = stat == Stat.Special_attack ? 1.5f : 1f;
        }

        int statVal = Stats[stat];

        if (stat == Stat.Speed)
            HeldItems?.OnModifySpd?.Invoke(statVal, this);

        // Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * statItem * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal * boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(Dictionary<Stat, int> boosts, Pokemon source, MoveBase move=null)
    {
        OnBoost(boosts, source);

        foreach (var kvp in boosts)
        {
            var stat = kvp.Key;
            var boost = kvp.Value;
            bool changeIsPositive = (boost > 0) ? true : false;
            
            if ((changeIsPositive && StatBoosts[stat] == 6) || (!changeIsPositive && StatBoosts[stat] == -6))
            {
                string riseOrFall = changeIsPositive ? "higher" : "lower";
                StatusChanges.Enqueue($"{Base.Name}'s {stat} won't go any {riseOrFall}!");
            }
            else
            {
                StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
                string riseOrFall = changeIsPositive ? "rose" : "fell";
                string bigChange = (Mathf.Abs(boost) >= 3) ? " severely " : (Mathf.Abs(boost) == 2) ? " harshly " : " ";
                StatusChanges.Enqueue($"{Base.Name}'s {stat}{bigChange}{riseOrFall}!");
            }

            Debug.Log($"{this.Base.Name} {stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            hasLeveled = true;
            level = Base.GrowthRate.LevelFromExp(Exp);
            CalculateStats();
            return true;
        }

        hasLeveled = false;
        return false;
    }

    public LearnableMoves GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumberOfMoves)
            return;

        Moves.Add(new Move(moveToLearn));
    }

    public bool HasMove(MoveBase moveToCheck) => Moves.Count(m => m.Base == moveToCheck) > 0;

    public Evolution CheckForEvolution()
    {
        if (hasLeveled == true)
            return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level);
        else return null;
    }

    public Evolution CheckForEvolution(ItemBase item) => Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);

    public Forms CheckForMega(ItemBase item) => Base.Forms.FirstOrDefault(e => e.HeldItem == item);

    public void Evolve(Evolution evolution)
    {
        _base = evolution.EvolvesInto;
        CalculateStats();
    }

    public void MegaEvolve(Forms mega)
    {
        _base = mega.PokemonForm;
        CalculateStats();
    }

    public void ResetMega(Forms pokemon)
    {
        _base = pokemon.PokemonForm;
    }

    public void Heal(int amount)
    {
        HP = amount;
        OnHPChanged?.Invoke();
        CureStatus();
    }

    public float GetNormalizedExp()
    {
        int currLevelExp = Base.GetExpForLevel(Level);
        int nextLevelExp = Base.GetExpForLevel(Level + 1);

        float normalizedExp = (float)(Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp(normalizedExp, 0, 1);
    }

    #region Properties
    public int Attack => GetStat(Stat.Attack);
    public int Defense => GetStat(Stat.Defense);
    public int SpAttack => GetStat(Stat.Special_attack);
    public int SpDefense => GetStat(Stat.Special_defense);
    public int Speed => GetStat(Stat.Speed);
    public int MaxHp { get; private set; }
    #endregion

    public float TypeEffectiveness(Move move)
    {
        return TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
    }

    public void TakeRecoilDamage(int damage)
    {
        if (damage < 1)
            damage = 1;
        DecreaseHP(damage);
        StatusChanges.Enqueue($"{Base.Name} was damaged by the recoil!");
    }

    public bool HasType(PokemonType t)
    {
        if ((_base.Type1 == t) || (_base.Type2 == t))
            return true;
        return false;
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public bool HasHpChanged() => OnHPChanged != null;

    public void SetStatus(Pokemon source, ConditionID conditionId, EffectData effect = null)
    {
        if (Status != null) return;

        bool canSet = Ability?.OnTrySetStatus?.Invoke(conditionId, this, source, effect) ?? true;
        if (!canSet) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this, source);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(Pokemon source, ConditionID conditionId, EffectData effect=null)
    {
        if (VolatileStatus != null) return;

        bool canSet = Ability?.OnTrySetVolatile?.Invoke(conditionId, this, source, effect) ?? true;
        if (!canSet) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this, source);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus() => VolatileStatus = null;

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return Moves[r];
    }

    public bool OnBeforeMove(Pokemon target, Pokemon source, Move move)
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null && Status?.OnBeforeMove(this) == false)
        {
            canPerformMove = false;
            Ability?.OnOpponentStatus?.Invoke(this);
        }

        if (Ability?.OnBeforeMove != null && Ability?.OnBeforeMove(target, source, move) == false)
        {
            canPerformMove = false;
            Ability?.OnBeforeMove?.Invoke(target, source, move);
        }

        if (VolatileStatus?.OnBeforeMove != null && VolatileStatus?.OnBeforeMove(this) == false)
            canPerformMove = false;

        if (HeldItems?.OnBeforeMove != null && HeldItems?.OnBeforeMove(this) == false)
            canPerformMove = false;

        if (move.Base.HasFlag(MoveFlag.FirstTurn) && !source.FirstTurn)
            canPerformMove = false;

        return canPerformMove;
    }

    public void OnAfterMove(Move move, Pokemon source, Pokemon target)
    {
        if (target.HeldItem != null)
        {
            if (target.HeldItem.Name == "Jaboca Berry")
                source.DecreaseHP(source.MaxHp / 8);
        }

        if (source.Ability?.OnAfterMove != null)
            source.Ability?.OnAfterMove?.Invoke(move.Base.Power, source, target, move.Base.Effects);
    }

    public void OnAfterTurn(Pokemon pokemon, Pokemon user)
    {
        if (pokemon.HeldItem != null)
            HeldItems?.OnAfterTurn?.Invoke(pokemon, user);

        Status?.OnAfterTurn?.Invoke(pokemon, user);
        VolatileStatus?.OnAfterTurn?.Invoke(pokemon, user);
        Ability?.OnAfterTurn?.Invoke(pokemon);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;

        if (isMega)
            ResetMega(_base.Forms[0]);

        isMega = false;

        ResetStatBoost();
        CalculateStats();
    }

    public void OnDamagingHit(float damage, Pokemon attacker, Move move)
    {
        Ability?.OnDamagingHit?.Invoke(damage, this, attacker, move);
        HeldItems?.OnDamagingHit?.Invoke(this, attacker, move);
    }

    public void OnBoost(Dictionary<Stat, int> boosts, Pokemon source) => Ability?.OnBoost?.Invoke(boosts, this, source);

    public float OnBasePower(float basePower, Pokemon attacker, Pokemon defender, Move move)
    {
        if (Ability?.OnBasePower != null)
            basePower = Ability.OnBasePower(basePower, attacker, defender, move);

        return basePower;
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
    public int DamageDealt { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int IV_HP;
    public int IV_ATK;
    public int IV_DEF;
    public int IV_SPA;
    public int IV_SPD;
    public int IV_SPE;
    public int EV_HP;
    public int EV_ATK;
    public int EV_DEF;
    public int EV_SPA;
    public int EV_SPD;
    public int EV_SPE;
    public string nature;
    public HeldItemID itemId;
    public string heldItem;
    public PokemonGender gender;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
    public AbilityID ability;
}
