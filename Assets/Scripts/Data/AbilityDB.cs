using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDB
{
    private static BattleSystem _battleSystem;

    public static void Init()
    {
        _battleSystem = GameController.i.BattleSystem;

        foreach (var kvp in Abilities)
        {
            var abilityId = kvp.Key;
            var ability = kvp.Value;

            ability.Id = abilityId;
            ability.Source = EffectSource.Ability;
            ability.SourceId = (int)abilityId;
        }
    }

    public static Dictionary<AbilityID, Ability> Abilities { get; set; } = new Dictionary<AbilityID, Ability>()
    {
        // 1. Gen 3
        {
            AbilityID.stench,
            new Ability()
            {
                Name = "Stench",
                OutsideBattle = (Pokemon pokemon) =>
                {
                    bool stenchActive = false;

                    if (!TallGrass.i.halfEncounter)
                        stenchActive = true;

                    return stenchActive;
                },
                OnAfterMove = (int power, Pokemon attacker, Pokemon target, EffectData effects) =>
                {
                    // 10% chance to cause Flinch when hit with damaging moves
                    if (power > 0 && Random.Range(1, 11) == 1)
                        target.SetVolatileStatus(attacker, ConditionID.flinch, effects);
                }
            }
        },
        {
            AbilityID.drizzle,
            new Ability()
            {
                Name = "Drizzle",
                OnPokemonEnter = (Pokemon source, Pokemon target) =>
                {
                    var field = _battleSystem.Field;

                    field.WeatherDuration = 5;
                    field.SetWeather(source, ConditionID.rain);

                    Debug.Log("Rain is called by Drizzle");
                }
            }
        },
        {
            AbilityID.speed_boost,
            new Ability()
            {
                Name = "Speed Boost",
                OnAfterTurn = (Pokemon attacker) =>
                {
                    Dictionary<Stat, int> boost = new Dictionary<Stat, int>()
                    {
                        { Stat.Speed, 1 }
                    };

                    attacker.ApplyBoosts(boost, attacker);
                }
            }
        },
        {
            AbilityID.battle_armor,
            new Ability()
            {
                Name = "Battle Armor",
                OnPreventMove = (attacker, defender, move) =>
                {
                    var preFocus = attacker.CurrentMove.Base.CritBehavior;
                    attacker.CurrentMove.Base.CritBehavior = CritBehavior.NeverCrits;

                    return false;
                }
            }
        },
        {
            AbilityID.sturdy,
            new Ability()
            {
                Name = "Sturdy",
                OnBeforeDamage = (float damage, Pokemon target, Pokemon source, Move move) =>
                {
                    damage = target.MaxHp - 1;

                    if (target.HP - damage == 1)
                    {
                        target.StatusChanges.Enqueue($"{target.Base.Name} survived with 1 HP!");
                        Debug.Log("Sturdy activated");
                    }
                    
                    return (int)damage;
                }
            }
        },
        {
            AbilityID.damp,
            new Ability()
            {
                Name = "Damp",
                OnPreventMove = (target, source, move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Bomb))
                    {
                        source.StatusChanges.Enqueue($"{source.Base.Name} used {move.Base.Name}! " +
                            $"\n\n\nbut it was canceled by {target.Base.Name}'s ability!");
                        Debug.Log("Damp Activated");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            AbilityID.limber,
            new Ability()
            {
                Name = "Limber",
                OnTrySetStatus = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
                {
                    if (statusId != ConditionID.par)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        target.StatusChanges.Enqueue($"{target.Base.Name} is immune to paralysis");

                    return false;
                }
            }
        },
        {
            AbilityID.sand_veil,
            new Ability()
            {
                Name = "Sand Veil",
                OnModifyAcc = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (_battleSystem.Field.Weather != null)
                    {
                        if (_battleSystem.Field.Weather.Id == ConditionID.sandstorm)
                        {
                            acc = acc * .8f;
                            Debug.Log($"Sand Veil boost");
                        }
                    }

                    return acc;
                },
                OnBlockWeather = (Pokemon pokemon) =>
                {
                    return true;
                }
            }
        },
        {
            AbilityID._static,
            new Ability()
            {
                Name = "Static",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause paralyze when hit with contact moves
                    var flags = move.Base.Flags;
                    if (flags != null && flags.Contains(MoveFlag.Contact) && Random.Range(1, 4) == 1)
                    {
                        Debug.Log("Static causes Paralyze");
                        attacker.StatusChanges.Enqueue($"{attacker.Base.Name} is paralyzed by {target.Base.Name}'s Static!");
                        attacker.SetStatus(attacker, ConditionID.par);
                    }
                }
            }
        },
        {
            AbilityID.volt_absorb,
            new Ability()
            {
                Name = "Volt Absorb",
                OnPreventMove = (Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause paralyze when hit with contact moves
                    if (move.Base.Type == PokemonType.Electric)
                    {
                        Debug.Log("Volt Absorb activated");

                        if (target.HP >= target.MaxHp)
                        {
                            attacker.StatusChanges.Enqueue($"{move.Base.Name} has no effect on {target.Base.Name}.");
                            return false;
                        }
                        else
                        {
                            target.IncreaseHP(target.MaxHp / 4);
                            if (target.HP > target.MaxHp)
                                target.HP = target.MaxHp;
                        }
                    }

                    return false;
                }
            }
        },
        {
            AbilityID.water_absorb,
            new Ability()
            {
                Name = "Water Absorb",
                OnPreventMove = (Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause paralyze when hit with contact moves
                    if (move.Base.Type == PokemonType.Water)
                    {
                        Debug.Log("Water Absorb activated");

                        if (target.HP >= target.MaxHp)
                        {
                            attacker.StatusChanges.Enqueue($"{move.Base.Name} has no effect on {target.Base.Name}.");
                            return false;
                        }
                        else
                        {
                            target.IncreaseHP(target.MaxHp / 4);
                            if (target.HP > target.MaxHp)
                                target.HP = target.MaxHp;
                        }
                    }

                    return false;
                }
            }
        },
        {
            AbilityID.oblivious,
            new Ability()
            {
                Name = "Oblivious",
                OnTrySetVolatile = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
                {
                    if (statusId != ConditionID.infatuation)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move || effect.Source == EffectSource.Ability)
                    {
                        if (statusId == ConditionID.infatuation)
                        {
                            target.StatusChanges.Enqueue($"{target.Base.Name} is OBLIVIOUS to falling in love!");
                            Debug.Log("Oblivious prevents Infatuation");
                        }

                        if (statusId == ConditionID.taunt)
                        {
                            target.StatusChanges.Enqueue($"{target.Base.Name} is OBLIVIOUS to being taunted!");
                            Debug.Log("Oblivious prevents Taunt");
                        }
                    }

                    return false;
                }
            }
        },
        {
            AbilityID.cute_charm,
            new Ability()
            {
                Name = "Cute Charm",
                Effects = new EffectData
                {
                    Source = EffectSource.Ability,
                    SourceId = (int)AbilityID.cute_charm
                },
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to infatuate when hit with contact moves
                    var flags = move.Base.Flags;

                    if (flags != null && flags.Contains(MoveFlag.Contact) && Random.Range(1, 4) == 1 &&
                    target.Gender != attacker.Gender && target.Gender != PokemonGender.Genderless)
                    {
                        Debug.Log("Cute Charm causes infatuation");
                        attacker.StatusChanges.Enqueue($"{attacker.Base.Name} has fallen in love!");
                        attacker.SetVolatileStatus(attacker, ConditionID.infatuation, target.Ability.Effects);
                    }
                }
            }
        },
        {
            AbilityID.cloud_nine,
            new Ability()
            {
                Name = "Cloud Nine",
                Effects = new EffectData
                {
                    Source = EffectSource.Ability,
                    SourceId = (int)AbilityID.cloud_nine
                },
                OnPokemonEnter = (Pokemon source, Pokemon target) =>
                {
                    source.StatusChanges.Enqueue($"The effects of weather disappeared.");
                }
            }
        },
        {
            AbilityID.compound_eyes,
            new Ability()
            {
                Name = "Compound Eyes",
                OnModifyAcc = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    acc = acc * 1.3f;
                    Debug.Log($"Compound Eyes boost");

                    return acc;
                }
            }
        },
        {
            AbilityID.insomnia,
            new Ability()
            {
                Name = "Insomnia",
                OnTrySetStatus = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
                {
                    if (statusId != ConditionID.slp)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        target.StatusChanges.Enqueue($"{target.Base.Name} is immune to sleep");

                    return false;
                }
            }
        },
        {
            AbilityID.color_change,
            new Ability()
            {
                Name = "Color Change",
                OnDamagingHit = (float damage, Pokemon source, Pokemon target, Move move) =>
                {
                    if (damage > 0 && !source.HasType(move.Base.Type))
                    {
                        source.Base.Type1 = move.Base.Type;
                        source.Base.Type2 = PokemonType.None;
                        source.StatusChanges.Enqueue($"{source.Base.Name}'s type has changed to {move.Base.Type}.");
                    }
                }
            }
        },
        {
            AbilityID.immunity,
            new Ability()
            {
                Name = "Immunity",
                OnTrySetStatus = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
                {
                    if (statusId != ConditionID.psn)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        target.StatusChanges.Enqueue($"{target.Base.Name} is immune to poison");

                    return false;
                }
            }
        },
        {
            AbilityID.flash_fire,
            new Ability()
            {
                Name = "Flash Fire",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Fire)
                    {
                        basePower = basePower * 1.5f;
                        Debug.Log($"Flash Fire boost");
                    }

                    return basePower;
                }
            }
        },
        {
            AbilityID.shield_dust,
            new Ability()
            {
                Name = "Shield Dust",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    if (source.CurrentMove.Base.Power > 0)
                    {
                        foreach (var boost in boosts)
                        {
                            if (boost.Value < 0)
                            {
                                boosts.Remove(boost.Key);
                                target.StatusChanges.Enqueue($"{target.Base.Name}'s {boost.Key} cannot be decreased due to it's Shield Dust");
                            }
                        }
                    }
                },
                OnTrySetStatus = (ConditionID condition, Pokemon target, Pokemon source, EffectData effects) =>
                {
                    var effect = source.CurrentMove.Base.Effects;

                    if (effect != null && source.CurrentMove.Base.Power > 0)
                    {
                        if (effect.Status != ConditionID.none)
                        {
                            target.StatusChanges.Enqueue($"Shield Dust protected {target.Base.Name} from {effect.Status}!");
                            return false;
                        }
                    }

                    var secondaries = source.CurrentMove.Base.Secondaries;

                    if (secondaries != null && source.CurrentMove.Base.Power > 0)
                    {
                        foreach (var sec in secondaries)
                        {
                            if (sec.Status != ConditionID.none)
                            {
                                target.StatusChanges.Enqueue($"Shield Dust protected {target.Base.Name} from {sec.Status}!");
                                return false;
                            }
                        }
                    }

                    return true;
                },
                OnTrySetVolatile = (ConditionID condition, Pokemon target, Pokemon source, EffectData effects) =>
                {
                    var effect = source.CurrentMove.Base.Effects;

                    if (effect != null && source.CurrentMove.Base.Power > 0)
                    {
                        if (effect.VolatileStatus != ConditionID.none)
                        {
                            target.StatusChanges.Enqueue($"Shield Dust protected {target.Base.Name} from {effect.VolatileStatus}!");
                            return false;
                        }
                    }

                    var secondaries = source.CurrentMove.Base.Secondaries;

                    if (secondaries != null && source.CurrentMove.Base.Power > 0)
                    {
                        foreach (var sec in secondaries)
                        {
                            if (sec.VolatileStatus != ConditionID.none)
                            {
                                target.StatusChanges.Enqueue($"Shield Dust protected {target.Base.Name} from {sec.VolatileStatus}!");
                                return false;
                            }
	                    }
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.own_tempo,
            new Ability()
            {
                Name = "Own Tempo",
                OnTrySetVolatile = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
                {
                    if (statusId != ConditionID.confusion)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        target.StatusChanges.Enqueue($"{target.Base.Name} is immune to confusion");

                    return true;
                }
            }
        },
        //{
        //    AbilityID.suction_cups,
        //    new Ability()
        //    {
        //        Name = "Suction Cups",
        //        OnTrySetVolatile = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
        //        {
        //            if (statusId != ConditionID.confusion)
        //                return true;

        //            if (effect != null && effect.Source == EffectSource.Move)
        //                target.StatusChanges.Enqueue($"{target.Base.Name} is immune to confusion");

        //            return true;
        //        }
        //    }
        //},
        {
            AbilityID.intimidate,
            new Ability()
            {
                Name = "Intimidate",
                OnPokemonEnter = (Pokemon target, Pokemon source) =>
                {
                    if (target.Ability.Id != AbilityID.inner_focus)
                    {
                        Dictionary<Stat, int> boost = new Dictionary<Stat, int>()
                        {
                            { Stat.Attack, -1 }
                        };

                        target.ApplyBoosts(boost, target);
                    }
                }
            }
        },
        //{
        //    AbilityID.shadow_tag,
        //    new Ability()
        //    {
        //        Name = "ShadowTag",
        //        OnPokemonEnter = (Pokemon target, Pokemon source) =>
        //        {
        //            if (target.Ability.Id != AbilityID.inner_focus)
        //            {
        //                Dictionary<Stat, int> boost = new Dictionary<Stat, int>()
        //                {
        //                    { Stat.Attack, -1 }
        //                };

        //                target.ApplyBoosts(boost, target);
        //            }
        //        }
        //    }
        //},
        {
            AbilityID.rough_skin,
            new Ability()
            {
                Name = "Rough Skin",
                OnDamagingHit = (float dmg, Pokemon source, Pokemon attacker, Move move) =>
                {
                    attacker.StatusChanges.Enqueue($"{attacker.Base.Name} is damaged by {source.Base.Name}'s Rough Skin.");
                    attacker.DecreaseHP(attacker.MaxHp / 16);
                    Debug.Log($"Rough Skin damage to attacker");
                }
            }
        },
        {
            AbilityID.wonder_guard,
            new Ability()
            {
                Name = "Wonder Guard",
                OnBeforeMove = (Pokemon target, Pokemon source, Move move) =>
                {
                    if (TypeChart.GetEffectiveness(move.Base.Type, source.Base.Type1) != 2f)
                    {
                        target.StatusChanges.Enqueue($"{target.Base.Name} is not affected by {move.Base.Name}!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.levitate,
            new Ability()
            {
                Name = "Levitate",
                OnBeforeMove = (Pokemon target, Pokemon source, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Ground)
                    {
                        target.StatusChanges.Enqueue($"It doesn't affect {target.Base.Name}!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.effect_spore,
            new Ability()
            {
                Name = "Effect Spore",
                OnAfterMove = (int dmg, Pokemon source, Pokemon target, EffectData effects) =>
                {
                    // 10% chance to cause a status effect when hit with damaging moves
                    if (source.CurrentMove.Base.HasFlag(MoveFlag.Contact) && Random.Range(1, 11) == 1)
                    {
                        int r = Random.Range(1, 101);
                        if (r < 33)
                            target.SetVolatileStatus(source, ConditionID.psn, effects);
                        else if (r > 66)
                            target.SetVolatileStatus(source, ConditionID.par, effects);
                        else
                            target.SetVolatileStatus(source, ConditionID.slp, effects);
                    }
                }
            }
        },
        {
            AbilityID.synchronize,
            new Ability()
            {
                Name = "Synchronize",
                OnAfterMove = (int dmg, Pokemon source, Pokemon target, EffectData effects) =>
                {
                    ConditionID[] status = { ConditionID.psn, ConditionID.brn, ConditionID.par };
                    if (status.Contains(source.Status.Id))
                    {
                        target.SetStatus(target, source.Status.Id);
                    }
                }
            }
        },
        {
            AbilityID.clear_body,
            new Ability()
            {
                Name = "Clear Body",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    // If it's self boost then return
                    if (source != null && target == source) return;

                    bool showMsg = false;

                    foreach (var stat in boosts.Keys) {
                        if (boosts[stat] < 0)
                        {
                            showMsg = true;
                            boosts.Remove(Stat.Accuracy);
                        }
                    }

                    if (showMsg)
                    {
                        target.StatusChanges.Enqueue($"{target.Base.Name}'s Clear Body prevents stat loss");
                    }
                }
            }
        },

        // 1. Abilities that modify stats
        {
            AbilityID.adaptability,
            new Ability()
            {
                Name = "Adaptability",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (attacker.HasType(move.Base.Type))
                    {
                        atk = atk * 2f;
                        Debug.Log($"Adaptability boost");
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (attacker.HasType(move.Base.Type))
                    {
                        atk = atk * 2f;
                        Debug.Log($"Adaptability boost");
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.aerilate,
            new Ability()
            {
                Name = "Aerilate",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Normal)
                    {
                        move.ChangeMoveType(PokemonType.Normal, PokemonType.Flying);
                        atk = atk * 1.2f;
                        Debug.Log($"Aerilate boost");
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Normal)
                    {
                        move.ChangeMoveType(PokemonType.Normal, PokemonType.Flying);
                        atk = atk * 1.2f;
                        Debug.Log($"Aerilate boost");
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.overgrow,
            new Ability()
            {
                Name = "Overgrow",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Overgrow boost");
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Overgrow boost");
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.blaze,
            new Ability()
            {
                Name = "Blaze",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Blaze boost");
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Blaze boost");
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.torrent,
            new Ability()
            {
                Name = "Torrent",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Torrent boost");
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Torrent boost");
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.swarm,
            new Ability()
            {
                Name = "Swarm",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Swarm boost");
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                        Debug.Log($"Swarm boost");
                    }

                    return atk;
                }
            }
        },

        // 2. Abilities that prevent stat boost
        {
            AbilityID.keen_eye,
            new Ability()
            {
                Name = "Keen Eye",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    // If it's self boost then return
                    if (source != null && target == source) return;

                    if (boosts.ContainsKey(Stat.Accuracy) && boosts[Stat.Accuracy] < 0)
                    {
                        boosts.Remove(Stat.Accuracy);

                        target.StatusChanges.Enqueue($"{target.Base.Name}'s accuracy cannot be decreased due to it's keen eye");
                    }
                }
            }
        },
        {
            AbilityID.hyper_cutter,
            new Ability()
            {
                Name = "Hyper Cutter",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    // If it's self boost then return
                    if (source != null && target == source) return;

                    if (boosts.ContainsKey(Stat.Attack) && boosts[Stat.Attack] < 0)
                    {
                        boosts.Remove(Stat.Attack);

                        target.StatusChanges.Enqueue($"{target.Base.Name}'s attack cannot be decreased");
                    }
                }
            }
        },

        // 3. Abilities that prevent inflicting status conditions
        {
            AbilityID.vital_spirit,
            new Ability()
            {
                Name = "Vital Spirit",
                OnTrySetStatus = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
                {
                    if (statusId != ConditionID.slp)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        target.StatusChanges.Enqueue($"{target.Base.Name} is immune to sleep");

                    return false;
                }
            }
        },
        {
            AbilityID.water_veil,
            new Ability()
            {
                Name = "Water Veil",
                OnTrySetStatus = (ConditionID statusId, Pokemon target, Pokemon source, EffectData effect) =>
                {
                    if (statusId != ConditionID.brn)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        target.StatusChanges.Enqueue($"{target.Base.Name} is immune to burn");

                    return false;
                }
            }
        },

        // 4. Abilities that modify base power of moves
        {
            AbilityID.iron_fist,
            new Ability()
            {
                Name = "Iron Fist",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Punch))
                    {
                        basePower = basePower * 1.2f;
                        Debug.Log($"Iron Fist boost");
                    }

                    return basePower;
                }
            }
        },
        {
            AbilityID.strong_jaw,
            new Ability()
            {
                Name = "Strong Jaw",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Bite))
                    {
                        basePower = basePower * 1.5f;
                        Debug.Log($"Strong Jaw boost");
                    }

                    return basePower;
                }
            }
        },
        {
            AbilityID.tough_claws,
            new Ability()
            {
                Name = "Tough Claws",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Contact))
                    {
                        Debug.Log("Tough Claw Boost");
                        basePower = basePower * 1.3f;
                    }

                    return basePower;
                }
            }
        },
        {
            AbilityID.mega_launcher,
            new Ability()
            {
                Name = "Mega Launcher",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Pulse))
                    {
                        basePower = basePower * 1.5f;
                        Debug.Log($"Mega Launcher boost");
                    }

                    return basePower;
                }
            }
        },

        {
            AbilityID.analytic,
            new Ability()
            {
                Name = "Analytic",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (!attacker.MovesFirst)
                    {
                        basePower = basePower * 1.3f;
                        Debug.Log($"Analytic boost");
                    }

                    return basePower;
                }
            }
        },

        // 5. Abilities that have effect during damaging hits
        {
            AbilityID.poison_point,
            new Ability()
            {
                Name = "Poison Point",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause poison when hit with contact moves
                    var flags = move.Base.Flags;
                    if (flags != null && flags.Contains(MoveFlag.Contact) && Random.Range(1, 4) == 1)
                    {
                        Debug.Log("Poison Point causes Poison");
                        attacker.StatusChanges.Enqueue($"{attacker.Base.Name} is poisoned by {target.Base.Name}'s Poison Point!");
                        attacker.SetStatus(attacker, ConditionID.psn);
                    }
                }
            }
        },
        {
            AbilityID.flame_body,
            new Ability()
            {
                Name = "Flame Body",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause burn when hit with contact moves
                    var flags = move.Base.Flags;
                    if (flags != null && flags.Contains(MoveFlag.Contact) && Random.Range(1, 4) == 1)
                    {
                        Debug.Log("Flamebody causes Burn");
                        attacker.StatusChanges.Enqueue($"{attacker.Base.Name} is burned by {target.Base.Name}'s Flame Body!");
                        attacker.SetStatus(attacker, ConditionID.brn);
                    }
                }
            }
        },

        // 6. Abilities that trigger on fainted Pokemon
        {
            AbilityID.aftermath,
            new Ability()
            {
                Name = "Aftermath",
                OnFaintedPokemon = (Pokemon target, Pokemon attacker, Move move) =>
                {
                    var flags = move.Base.Flags;
                    if (flags != null && flags.Contains(MoveFlag.Contact) && target.HP <= 0)
                    {
                        Debug.Log("Aftermath damage");
                        int damage = attacker.HP / 4;
                        attacker.StatusChanges.Enqueue($"{attacker.Base.Name} is damaged in the Aftermath!");
                        attacker.DecreaseHP(damage);
                    }
                }
            }
        },

        // 7. Abilities that use opponent's status
        {
            AbilityID.bad_dreams,
            new Ability()
            {
                Name = "Bad Dreams",
                OnOpponentStatus = (Pokemon target) =>
                {
                    if (target.Status.Id == ConditionID.slp)
                    {
                        Debug.Log("Bad Dreams damage");
                        int damage = target.HP / 8;
                        target.StatusChanges.Enqueue($"{target.Base.Name} is damaged in the Aftermath!");
                        target.DecreaseHP(damage);
                    }
                }
            }
        },

        // 8. Abilities that happen when Pokemon enters battle
        
        {
            AbilityID.anticipation,
            new Ability()
            {
                Name = "Anticipation",
                OnPokemonEnter = (Pokemon target, Pokemon source) =>
                {
                    var moves = target.Moves;
                    foreach (var move in moves)
                    {
                        float type = TypeChart.GetEffectiveness(move.Base.Type, source.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, source.Base.Type2);
                        if (type == 2f ||
                            move.Base.Name == "Explosion" ||
                            move.Base.Name == "Self Destruct" ||
                            move.Base.HasFlag(MoveFlag.InstantKO))
                        {
                            target.StatusChanges.Enqueue($"{source.Base.Name} shudders.");
                        }
                    }
                }
            }
        },


        // 9. Abilities that affect owner Pokemon's stats
        {
            AbilityID.moxie,
            new Ability()
            {
                Name = "Moxie",
                OnFaintedPokemon = (Pokemon fainted, Pokemon attacker, Move move) =>
                {
                    if (fainted.HP <= 0)
                    {
                        Debug.Log("Moxie boost");
                        Dictionary<Stat, int> boost = new Dictionary<Stat, int>()
                        {
                            { Stat.Attack, 1 }
                        };

                        attacker.ApplyBoosts(boost, attacker);
                    }
                }
            }
        },
        {
            AbilityID.anger_point,
            new Ability()
            {
                Name = "Anger Point",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    if (target.CritTaken)
                    {
                        Dictionary<Stat, int> boost = new Dictionary<Stat, int>()
                        {
                            { Stat.Attack, 6 }
                        };

                        target.ApplyBoosts(boost, target);
                        target.CritTaken = false;
                    }
                }
            }
        },
        {
            AbilityID.anger_shell,
            new Ability()
            {
                Name = "Anger Shell",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    if (target.HP < target.MaxHp / 2 && move != null)
                    {
                        Dictionary<Stat, int> boost = new Dictionary<Stat, int>()
                        {
                            { Stat.Attack, 1 },
                            { Stat.SpAttack, 1 },
                            { Stat.Speed, 1 },
                            { Stat.Defense, -1 },
                            { Stat.SpDefense, -1 }
                        };

                        target.ApplyBoosts(boost, target);
                        target.CritTaken = false;
                    }
                }
            }
        },
        {
            AbilityID.berserk,
            new Ability()
            {
                Name = "Berserk",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    if (target.HP < target.MaxHp / 2 && move != null)
                    {
                        Dictionary<Stat, int> boost = new Dictionary<Stat, int>()
                        {
                            { Stat.SpAttack, 1 }
                        };

                        target.ApplyBoosts(boost, target);
                    }
                }
            }
        },

        // 10. Move prevention/altering abilities
        {
            AbilityID.armor_tail,
            new Ability()
            {
                Name = "Armor Tail",
                OnPreventMove = (target, source, move) =>
                {
                    if (move.Base.Priority > 0)
                    {
                        source.StatusChanges.Enqueue($"{source.Base.Name} cannot use {move.Base.Name}!");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            AbilityID.bulletproof,
            new Ability()
            {
                Name = "Bulletproof",
                OnPreventMove = (target, source, move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Bullet))
                    {
                        target.StatusChanges.Enqueue($"{source.Base.Name} is unaffected by {move.Base.Name}!");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            AbilityID.big_pecks,
            new Ability()
            {
                Name = "Big Pecks",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    if (move.Base.Effects.Boosts.Count > 0)
                    {
                        foreach (var boost in move.Base.Effects.Boosts)
                        {
                            if (boost.Stat == Stat.Defense && boost.Boost < 0)
                            {
                                Dictionary<Stat, int> newBoost = new Dictionary<Stat, int>()
                                {
                                    { boost.Stat, Mathf.Abs(boost.Boost) }
                                };

                                target.ApplyBoosts(newBoost, target);
                            }
                        }
                    }
                }
            }
        },

        // 11. Trapping abilities
        {
            AbilityID.arena_trap,
            new Ability()
            {
                Name = "Arena Trap",
                OnTrapped = (target, source) =>
                {
                    if (target.HasType(PokemonType.Flying) || target.HasType(PokemonType.Ghost) || target.Ability.Id == AbilityID.levitate)
                        return false;
                    else
                        return true;
                }
            }
        },
        {
            AbilityID.shadow_tag,
            new Ability()
            {
                Name = "Shadow Tag",
                OnTrapped = (target, source) =>
                {
                    if (target.Ability.Id == AbilityID.shadow_tag)
                        return false;
                    else
                        return true;
                }
            }
        },
        {
            AbilityID.magnet_pull,
            new Ability()
            {
                Name = "Magnet Pull",
                OnTrapped = (target, source) =>
                {
                    if (target.HasType(PokemonType.Steel))
                        return false;
                    else
                        return true;
                }
            }
        },

        // 12. Weather abilities

        // 13. Abilities that need implementing
        {
            AbilityID.aroma_veil,
            new Ability()
            {
                Name = "Aroma Veil",
                // OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                // {
                    // TODO: Set Aroma Veil
                    // if pokemon uses move -
                    //      Taunt, Torment, Encore, Disable, Cursed Body
                    //      Heal Block, Infatuation.
                    // prevent the moves use.

                    // set up Taunt ConditionID
                // }
            }
        },
        {
            AbilityID.as_one_a,
            new Ability()
            {
                Name = "As One",
                // OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                // {
                    // TODO: Implement Unburden & Chilling Neigh
                // }
            }
        },
        {
            AbilityID.as_one_b,
            new Ability()
            {
                Name = "As One",
                // OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                // {
                    // TODO: Implement Unburden & Grim Neigh
                // }
            }
        },
        {
            AbilityID.aura_break,
            new Ability()
            {
                Name = "Aura Break",
                // OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                // {
                    // TODO: Implement Aura moves
                    // add Aura to HasFlag enum in MoveBase
                // }
            }
        },
        {
            AbilityID.ball_fetch,
            new Ability()
            {
                Name = "Ball Fetch",
                // OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                // {
                    // TODO: Implement Held Items
                // }
            }
        },
        {
            AbilityID.battery,
            new Ability()
            {
                Name = "Battery",
                // OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                // {
                    // TODO: Implement Unburden & Grim Neigh
                // }
            }
        },
        {
            AbilityID.tinted_lens,
            new Ability()
            {
                Name = "Tinted Lens",
                // OnBeforeMove = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                // {
                    // TODO: Implement method allow "not very effective" moves to do normal damage.
                    // Look into typeEffectiveness class? in PokemonBase
                // }
            }
        },
        {
            AbilityID.inner_focus,
            new Ability()
            {
                Name = "Inner Focus",
                //OnAfterMoveDefensive = (attacker, defender, move) =>
                //{
                    // TODO: Implement method for detecting attacker ability,
                    // then determining whether it can cause flinch,
                    // then reverting flinch to null
                //}
            }
        },
        {
            AbilityID.tangled_feet,
            new Ability()
            {
                Name = "Tangled Feet",
                //OnModifyAEva = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                //{
                    // TODO: Add method to modify evasiveness
                //}
            }
        },
        {
            AbilityID.liquid_ooze,
            new Ability()
            {
                Name = "Liquid Ooze",
                //OnModifyAEva = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                //{
                    // TODO: Add method to modify evasiveness
                //}
            }
        },
        {
            AbilityID.guts,
            new Ability()
            {
                Name = "Guts",
                //OnModifyAEva = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                //{
                    // TODO: Add method to modify evasiveness
                //}
            }
        },
        {
            AbilityID.run_away,
            new Ability()
            {
                Name = "Run Away",
                //OnModifyAEva = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                //{
                    // TODO: Add method to modify evasiveness
                //}
            }
        },
        {
            AbilityID.sticky_hold,
            new Ability()
            {
                Name = "Sticky Hold",
                //OnModifyAEva = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                //{
                    // TODO: Add method to modify evasiveness
                //}
            }
        },
    };
}

public enum AbilityID
{
    none, adaptability, aerilate, aftermath, air_lock, analytic, anger_point, anger_shell, anticipation,
    arena_trap, armor_tail, aroma_veil, as_one_a, as_one_b, aura_break, bad_dreams, ball_fetch, battery, battle_armor, 
    battle_bond, beads_of_ruin, beast_boost, berserk, big_pecks, blaze, bulletproof, cheek_pouch, chilling_neigh,
    chlorophyll, clear_body, cloud_nine, color_change, comatose, commander, competitive, compound_eyes, contrary,
    corrosion, costar, cotton_down, cud_chew, curious_medicine, cursed_body, cute_charm, damp, dancer, dark_aura,
    dauntless_shield, dazzling, defeatist, defiant, delta_stream, desolte_land, disguise, download, dragons_maw, 
    drizzle, drought, dry_skin, early_bird, earth_eater, effect_spore, electric_surge, electromorphosis, 
    embody_aspect, emergency_exit, fairy_aura, filter, flame_body, flare_boost, flash_fire, flower_gift, 
    flower_veil, fluffy, forecast, forewarn, friend_guard, frisk, full_metal_body, fur_coat, gale_wings, galvanize,
    gluttony, good_as_gold, gooey, gorilla_tactics, grass_pelt, grassy_surge, grim_neigh, guard_dog, gulp_missle,
    guts, hadron_engine, harvest, healer, heatproof, heavy_metal, honey_gather, hospitality, huge_power,
    hunger_switch, hustle, hydration, hyper_cutter, ice_body, ice_face, ice_scales, illuminate, illusion, immunity, 
    imposter, infiltrator, innards_out, inner_focus, insomnia, intimidate, intrepid_sword, iron_barbs, iron_fist,
    justified, keen_eye, klutz, leaf_guard, levitate, libero, light_metal, lightning_rod, limber, lingering_aroma, 
    liquid_ooze, liquid_voice, long_reach, magic_bounce, magic_guard, magician, magma_armor, magnet_pull, marvel_scale,
    mega_launcher, merciless, mimicry, minds_eye, minus, mirror_armor, misty_surge, mold_breaker, moody, motor_drive,
    moxie, multiscale, multitype, mummy, mycelium_might, natural_cure, neuroforce, neutralizing_gas, no_guard, 
    normalize, oblivious, opportunist, orichalcum_pulse, overcoat, overgrow, own_tempo, parental_bond, pastel_veil,
    perish_body, pickpocket, pickup, pixilate, plus, poison_heal, poison_point, poison_puppeteer, poison_touch,
    power_construct, power_of_alchemy, power_spot, prankster, pressure, primordial_sea, prism_armor, propeller_tail,
    protean, protosynthesis, psychic_surge, punk_rock, pure_power, purifying_salt, quark_drive, queenly_majesty, 
    quick_draw, quick_feet, rain_dish, rattled, receiver, reckless, refrigerate, regenerator, ripen, rivalry, rks_system, 
    rock_head, rocky_payload, rough_skin, run_away, sand_force, sand_rush, sand_spit, sand_stream, sand_veil, 
    sap_sipper, schooling, scrappy, screen_cleaner, seed_sower, serene_grace, shadow_shield, shadow_tag, sharpness,
    shed_skin, sheer_force, shell_armor, shield_dust, shields_down, simple, skill_link, slow_start, slucsh_rush,
    sniper, snow_cloak, snow_warning, solar_power, solid_rock, soul_heart, soundproof, speed_boost, stakeout,
    stall, stalwart, stamina, stance_change, _static, steadfast, steam_engine, steelworker, steely_spirit, stench,
    sticky_hold, storm_drain, strong_jaw, sturdy, suction_cups, super_luck, supersweet_syrup, supreme_overlord,
    surge_surfer, swarm, sweet_veil, swift_swim, sword_of_ruin, symbiosis, synchronize, tablets_of_ruin, tangled_feet,
    tangling_hair, technician, telepathy, tera_sheel, tera_shift, teraform_zero, teravolt, thermal_exchange, thick_fat,
    tinted_lens, torrent, tough_claws, toxic_boost, toxic_chain, toxic_debris, trace, transistor, triage, truant,
    turboblaze, unaware, unburden, unnerve, unseen_fist, vessel_of_ruin, victory_star, vital_spirit, volt_absorb, 
    wandering_spirit, water_absorb, water_bubble, water_compaction, water_veil, weak_armor, well_baked_body, white_smoke,
    wimp_out, wind_power, wind_rider, wonder_guard, wonder_skin, zen_mode, zero_to_hero
}
