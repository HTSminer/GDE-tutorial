using PKMNUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamageState : State<BattleSystem>
{
    public static TakeDamageState i { get; private set; }
    private void Awake() => i = this;

    // Outputs
    public DamageDetails DamageDetails { get; private set; } = new DamageDetails();

    // References
    private BattleSystem _battleSystem;
    private Pokemon _attacker;
    private Pokemon _defender;

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;

        _attacker = RunTurnState.i.CurrentAttacker.Pokemon;
        _defender = RunTurnState.i.CurrentDefender.Pokemon;

        OnTakeDamage(_attacker, _defender);
    }

    public DamageDetails OnTakeDamage(Pokemon attacker, Pokemon defender)
    {
        float critical = CritCalculation();
        float type = defender.TypeEffectiveness(attacker.CurrentMove);

        float weatherMod = _battleSystem.Field.Weather?.OnDamageModify?.Invoke(defender, attacker, attacker.CurrentMove) ?? 1f;

        DamageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false,
            DamageDealt = 0
        };

        float attack;
        float defense;
        if (attacker.CurrentMove.Base.Category == MoveCategory.Special)
        {
            attack = attacker.SpAttack;
            defense = defender.SpDefense;

            // Abilites & Held Items might modify the stats
            attack = ModifySpAtk(attack);
            defense = ModifySpDef(defense);
        }
        else
        {
            attack = attacker.Attack;
            defense = defender.Defense;

            // Abilites & Held might modify the stats
            attack = ModifyAtk(attack);
            defense = ModifyDef(defense);
        }

        // Abilities might modify base power
        int p = Mathf.FloorToInt(attacker.OnBasePower(attacker.CurrentMove.Base.Power, attacker, defender, attacker.CurrentMove));

        if (attacker.CurrentMove.Base.PowerBasedOn == PowerBasedOn.targetWeight)
            p = GetPowerFromBaseWeight();
        else if (attacker.CurrentMove.Base.PowerBasedOn == PowerBasedOn.weightDiff)
            p = GetPowerFromWeightDiff();
        else if (attacker.CurrentMove.Base.PowerBasedOn == PowerBasedOn.positiveStatusCount)
            p = StatusPowerMultiplier();
        else if (attacker.CurrentMove.Base.PowerBasedOn == PowerBasedOn.consecutiveMoveUse)
            p = ConsecutivePowerMultiplier();

        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * type * critical * weatherMod;

        float a = ((2 * attacker.Level) / 5 + 2);
        float d = (a * p * ((float)attack / defense)) / 50 + 2;

        int damage = Mathf.FloorToInt(d * modifiers);

        DamageDetails.DamageDealt = damage;

        damage = defender.HeldItems?.OnTakeDamage?.Invoke(damage, defender, attacker.CurrentMove) ?? damage;

        if (defender.Ability?.OnBeforeDamage != null)
            defender.DecreaseHP(defender.Ability.OnBeforeDamage.Invoke(damage, defender, attacker, attacker.CurrentMove));
        else
            defender.DecreaseHP(damage);

        if (damage > 0) defender.OnDamagingHit(damage, attacker, attacker.CurrentMove);

        _battleSystem.StateMachine.Pop();
        return DamageDetails;
    }

    public float ModifyAtk(float atk)
    {
        if (_defender.Ability?.OnModifyAtk != null)
            atk = _defender.Ability.OnModifyAtk(atk, _attacker, _defender, _attacker.CurrentMove);

        if (_attacker.HeldItems?.OnModifyAtk != null)
            atk = _attacker.HeldItems.OnModifyAtk(atk, _attacker, _defender, _attacker.CurrentMove);

        return atk;
    }

    public float ModifySpAtk(float atk)
    {
        if (_defender.Ability?.OnModifySpAtk != null)
            atk = _defender.Ability.OnModifySpAtk(atk, _attacker, _defender, _attacker.CurrentMove);

        return atk;
    }

    public float ModifyDef(float def)
    {
        if (_defender.Ability?.OnModifyDef != null)
            def = _defender.Ability.OnModifyDef(def, _attacker, _defender, _attacker.CurrentMove);

        return def;
    }

    public float ModifySpDef(float def)
    {
        if (_defender.Ability?.OnModifySpDef != null)
            def = _defender.Ability.OnModifySpDef(def, _attacker, _defender, _attacker.CurrentMove);

        return def;
    }

    public float ModifySpd(float spd)
    {
        if (_defender.Ability?.OnModifySpd != null)
            spd = _defender.Ability.OnModifySpd(spd, _attacker, _defender, _attacker.CurrentMove);

        return spd;
    }

    public float ModifyAcc(float acc, Pokemon source, Pokemon target)
    {
        if (target.Ability?.OnModifyAcc != null)
            acc = target.Ability.OnModifyAcc(acc, source, target, source.CurrentMove);

        return acc;
    }
    public float CritCalculation()
    {
        float critical = 1f;
        if (!(_attacker.CurrentMove.Base.CritBehavior == CritBehavior.NeverCrits))
        {
            if (_attacker.CurrentMove.Base.CritBehavior == CritBehavior.AlwaysCrits)
            {
                critical = 1.5f;
                _defender.CritTaken = true;
            }
        }
        else
        {
            int critChance = 0 + ((_attacker.CurrentMove.Base.CritBehavior == CritBehavior.HighCritRatio) ? 1 : 0);
            // TODO: Ability, HeldItem
            float[] chances = new float[] { (4.167f), (12.5f), (50f), 100f };
            if (Random.value * 100f <= chances[Mathf.Clamp(critChance, 0, 3)])
            {
                critical = 1.5f;
                _defender.CritTaken = true;
            }
        }
        return critical;
    }

    public int StatusPowerMultiplier()
    {
        int boosts = 1;
        foreach (var statBoost in _attacker.StatBoosts)
        {
            var boost = statBoost.Value;
            if (boost > 0)
                boosts++;
        }

        return _attacker.CurrentMove.Base.Power * boosts;
    }

    public int ConsecutivePowerMultiplier()
    {
        if (_attacker.PrevMoveUsed == _attacker.CurrentMove)
        {
            _attacker.SameMoveUsed++;
            return _attacker.CurrentMove.Base.Power * _attacker.SameMoveUsed;
        }

        return _attacker.CurrentMove.Base.Power;
    }

    public int GetPowerFromBaseWeight()
    {
        float w = _attacker.Base.Weight;
        if (w < 10f) { return 20; }
        else if (w < 25f) { return 40; }
        else if (w < 50f) { return 60; }
        else if (w < 100f) { return 80; }
        else if (w < 200f) { return 100; }
        else { return 120; }
    }

    public int GetPowerFromWeightDiff()
    {
        float defending = _defender.Base.Weight;
        float attacking = _attacker.Base.Weight;

        if (defending > (attacking * 0.5f))
            return 40;
        else if (defending > (attacking * 0.3335f))
            return 60;
        else if (defending > (attacking * 0.2501f))
            return 80;
        else if (defending > (attacking * 0.2001f))
            return 100;
        else
            return 120;
    }

}
