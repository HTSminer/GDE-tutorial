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

    public string Nature { get; set; }

    public Ability Ability { get; set; }

    public HeldItems HeldItems { get; set; }
    public ItemBase HeldItem;
    public bool HoldingItem { get; set; } = false;

    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public float SameMoveUsed { get; set; } = 1f;
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
                { Stat.SpAttack, 0 },
                { Stat.SpDefense, 0 },
                { Stat.Speed, 0 }
            };

        StatEffortValues = new Dictionary<Stat, int>()
        {
            { Stat.HitPoints, 0 },
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
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
            { Stat.SpAttack, saveData.IV_SPA },
            { Stat.SpDefense, saveData.IV_SPD },
            { Stat.Speed, saveData.IV_SPE }
        };
        StatEffortValues = new Dictionary<Stat, int>()
        {
            { Stat.HitPoints, saveData.EV_HP },
            { Stat.Attack, saveData.EV_ATK },
            { Stat.Defense, saveData.EV_DEF },
            { Stat.SpAttack, saveData.EV_SPA },
            { Stat.SpDefense, saveData.EV_SPD },
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
            IV_SPA = StatIndividualValues[Stat.SpAttack],
            IV_SPD = StatIndividualValues[Stat.SpDefense],
            IV_SPE = StatIndividualValues[Stat.Speed],
            EV_HP = StatEffortValues[Stat.HitPoints],
            EV_ATK = StatEffortValues[Stat.Attack],
            EV_DEF = StatEffortValues[Stat.Defense],
            EV_SPA = StatEffortValues[Stat.SpAttack],
            EV_SPD = StatEffortValues[Stat.SpDefense],
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
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt(Mathf.FloorToInt((((2 * Base.SpAttack) + StatIndividualValues[Stat.SpAttack] + (StatEffortValues[Stat.SpAttack] / 4)) * level) / 100 + 5) * Natures.GetNature(Nature).GetSPA()));
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt(Mathf.FloorToInt((((2 * Base.SpDefense) + StatIndividualValues[Stat.SpDefense] + (StatEffortValues[Stat.SpDefense] / 4)) * level) / 100 + 5) * Natures.GetNature(Nature).GetSPD()));
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
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
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
                statItem = stat == Stat.SpAttack ? 1.5f : 1f;
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

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            hasLeveled = true;
            ++level;
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
    public int SpAttack => GetStat(Stat.SpAttack);
    public int SpDefense => GetStat(Stat.SpDefense);
    public int Speed => GetStat(Stat.Speed);
    public int MaxHp { get; private set; }
    #endregion

    public DamageDetails TakeDamage (Move move, Pokemon attacker, Condition weather)
    {
        float critical = CritCalculation(move);
        float type = TypeEffectiveness(move);

        float weatherMod = weather?.OnDamageModify?.Invoke(this, attacker, move) ?? 1f;

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false,
            DamageDealt = 0
        };

        float attack;
        float defense;
        if (move.Base.Category == MoveCategory.Special)
        {
            attack = attacker.SpAttack;
            defense = SpDefense;

            // Abilites & Held Items might modify the stats
            attack = attacker.ModifySpAtk(attack, attacker, move);
            defense = ModifySpDef(defense, attacker, move);
        }
        else
        {
            attack = attacker.Attack;
            defense = Defense;

            // Abilites & Held might modify the stats
            attack = attacker.ModifyAtk(attack, attacker, move);
            defense = ModifyDef(defense, attacker, move);
        }

        // Abilities might modify base power
        int basePower = Mathf.FloorToInt(attacker.OnBasePower(move.Base.Power, attacker, this, move));

        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * type * critical * weatherMod;
        float statusPower = StatusPowerMultiplier(attacker, move);
        float consPower = ConsecutivePowerMultiplier(attacker, move);

        float a = ((2 * attacker.Level) / 5 + 2);
        float power = basePower * statusPower * consPower;
        float d = (a * power * ((float)attack / defense)) / 50 + 2;

        int damage = Mathf.FloorToInt(d * modifiers);

        damageDetails.DamageDealt = damage;

        damage = HeldItems?.OnTakeDamage?.Invoke(damage, this, move) ?? damage;

        if (Ability?.OnBeforeDamage != null)
            DecreaseHP(Ability.OnBeforeDamage.Invoke(damage, this, attacker, move));
        else
            DecreaseHP(damage);

        if (damage > 0) OnDamagingHit(damage, attacker, move);

        return damageDetails;
    }

    public float TypeEffectiveness(Move move)
    {
        return TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
    }

    public float CritCalculation(Move move)
    {
        float critical = 1f;
        if (!(move.Base.CritBehavior == CritBehavior.NeverCrits))
        {
            if (move.Base.CritBehavior == CritBehavior.AlwaysCrits)
            {
                critical = 1.5f;
                CritTaken = true;
            }
        }
        else
        {
            int critChance = 0 + ((move.Base.CritBehavior == CritBehavior.HighCritRatio) ? 1 : 0);
            // TODO: Ability, HeldItem
            float[] chances = new float[] { (4.167f), (12.5f), (50f), 100f };
            if (Random.value * 100f <= chances[Mathf.Clamp(critChance, 0, 3)])
            {
                critical = 1.5f;
                CritTaken = true;
            }
        }
        return critical;
    }

    public void TakeRecoilDamage(int damage)
    {
        if (damage < 1)
            damage = 1;
        DecreaseHP(damage);
        StatusChanges.Enqueue($"{Base.Name} was damaged by the recoil!");
    }

    public float StatusPowerMultiplier(Pokemon attacker, Move move)
    {
        float boosts = 1f;
        if (move.Base.HasFlag(MoveFlag.StatusMultiplier))
        {
            foreach (var statBoost in attacker.StatBoosts)
            {
                var boost = statBoost.Value;

                if (boost > 0)
                    boosts++;
            }
        }

        return boosts;
    }

    public float ConsecutivePowerMultiplier(Pokemon attacker, Move move)
    {
       if (move.Base.HasFlag(MoveFlag.ConsecutiveMoveUsedMultiplier) && attacker.PrevMoveUsed == move)
        {
            SameMoveUsed++;
            return SameMoveUsed;
        }
        else
            return 1f;
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

    public float ModifyAtk(float atk, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifyAtk != null)
            atk = Ability.OnModifyAtk(atk, attacker, this, move);

        if (attacker.HeldItems?.OnModifyAtk != null)
            atk = attacker.HeldItems.OnModifyAtk(atk, attacker, this, move);

        return atk;
    }

    public float ModifySpAtk(float atk, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpAtk != null)
            atk = Ability.OnModifySpAtk(atk, attacker, this, move);

        return atk;
    }

    public float ModifyDef(float def, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifyDef != null)
            def = Ability.OnModifyDef(def, attacker, this, move);

        return def;
    }

    public float ModifySpDef(float def, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpDef != null)
            def = Ability.OnModifySpDef(def, attacker, this, move);

        return def;
    }

    public float ModifySpd(float spd, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpd != null)
            spd = Ability.OnModifySpd(spd, attacker, this, move);

        return spd;
    }

    public float ModifyAcc(float acc, Pokemon attacker, Pokemon defender, Move move)
    {
        if (defender.Ability?.OnModifyAcc != null)
            acc = defender.Ability.OnModifyAcc(acc, attacker, defender, move);

        return acc;
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
