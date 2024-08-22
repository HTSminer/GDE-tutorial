using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldItemsDB
{
    public static void Init()
    {
        foreach (var kvp in HeldItems)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<HeldItemID, HeldItems> HeldItems { get; set; } = new Dictionary<HeldItemID, HeldItems>()
    {
        {
            HeldItemID.none,
            new HeldItems()
            {
                Name = "None",
                OnDamagingHit = (Pokemon defender, Pokemon attacker, Move move) =>
                {
                    if (attacker.HeldItem == null)
                    {
                        if (defender == null)
                        { 
                            //Debug.Log("Defender is null");
                        }
                        else if (defender.HeldItem == null)
                        { 
                            //Debug.Log("Defender's HeldItem is null");
                        }
                        else if (defender.HeldItem.HeldItemId == HeldItemID.sticky_barb)
                        {
                            attacker.HeldItem = defender.HeldItem;
                            defender.HeldItem = null;
                        }
                    }
                }
            }
        },

        #region Choice Items

        {
            HeldItemID.choice_band,
            new HeldItems()
            {
                Name = "Choice Band",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.PrevMoveUsed != null && pokemon.PrevMoveUsed == pokemon.CurrentMove)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} cannot use a different move due to its Choice Band.");
                        return true;
                    }
                    else
                        return false;
                },
                OnModifyAtk = (float atk, Pokemon source, Pokemon target, Move move) =>
                {
                    atk = atk * 1.5f;
                    return atk;
                }
            }
        },
        {
            HeldItemID.choice_scarf,
            new HeldItems()
            {
                Name = "Choice Scarf",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.PrevMoveUsed != null && pokemon.PrevMoveUsed == pokemon.CurrentMove)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} cannot use a different move due to its Choice Scarf.");
                        return true;
                    }
                    else
                        return false;
                },
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 1.5f;
                    Debug.Log("Choice Scarf Boost");
                    return spd;
                }
            }
        },
        {
            HeldItemID.choice_specs,
            new HeldItems()
            {
                Name = "Choice Specs",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.PrevMoveUsed != null && pokemon.PrevMoveUsed == pokemon.CurrentMove)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} cannot use a different move due to its Choice Specs.");
                        return true;
                    }
                    else
                        return false;
                },
                OnModifySpDef = (float spdef, Pokemon source) =>
                {
                    spdef = spdef * 1.5f;
                    Debug.Log("Choice Specs Boost");
                    return spdef;
                }
            }
        },

        #endregion

        #region Breeding / Training Items
        {
            HeldItemID.macho_brace,
            new HeldItems()
            {
                Name = "Macho Brace",
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 0.5f;
                    Debug.Log("Macho Brace speed reduction");
                    return spd;
                }
            }
        },
        {
            HeldItemID.power_anklet,
            new HeldItems()
            {
                Name = "Power Anklet",
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 0.5f;
                    Debug.Log("Power Anklet speed reduction");
                    return spd;
                },
                OnEVGain = (Pokemon source) =>
                {
                    var statEv = source.StatEffortValues;

                    statEv[Stat.Speed] = Mathf.Clamp((statEv[Stat.Speed] += 4), 0, GlobalSettings.i.MaxEvPerStat);

                    return statEv[Stat.Speed];
                }
            }
        },
        {
            HeldItemID.power_band,
            new HeldItems()
            {
                Name = "Power Band",
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 0.5f;
                    Debug.Log("Power Band speed reduction");
                    return spd;
                },
                OnEVGain = (Pokemon source) =>
                {
                    var statEv = source.StatEffortValues;

                    statEv[Stat.Special_defense] = Mathf.Clamp((statEv[Stat.Special_defense] += 4), 0, GlobalSettings.i.MaxEvPerStat);

                    return statEv[Stat.Special_defense];
                }
            }
        },
        {
            HeldItemID.power_belt,
            new HeldItems()
            {
                Name = "Power Belt",
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 0.5f;
                    Debug.Log("Power Belt speed reduction");
                    return spd;
                },
                OnEVGain = (Pokemon source) =>
                {
                    var statEv = source.StatEffortValues;

                    statEv[Stat.Defense] = Mathf.Clamp((statEv[Stat.Defense] += 4), 0, GlobalSettings.i.MaxEvPerStat);

                    return statEv[Stat.Defense];
                }
            }
        },
        {
            HeldItemID.power_bracer,
            new HeldItems()
            {
                Name = "Power Bracer",
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 0.5f;
                    Debug.Log("Power Bracer speed reduction");
                    return spd;
                },
                OnEVGain = (Pokemon source) =>
                {
                    var statEv = source.StatEffortValues;

                    statEv[Stat.Attack] = Mathf.Clamp((statEv[Stat.Attack] += 4), 0, GlobalSettings.i.MaxEvPerStat);

                    return statEv[Stat.Attack];
                }
            }
        },
        {
            HeldItemID.power_lens,
            new HeldItems()
            {
                Name = "Power Lens",
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 0.5f;
                    Debug.Log("Power Lens speed reduction");
                    return spd;
                },
                OnEVGain = (Pokemon source) =>
                {
                    var statEv = source.StatEffortValues;

                    statEv[Stat.Special_attack] = Mathf.Clamp((statEv[Stat.Special_attack] += 4), 0, GlobalSettings.i.MaxEvPerStat);

                    return statEv[Stat.Special_attack];
                }
            }
        },
        {
            HeldItemID.power_weight,
            new HeldItems()
            {
                Name = "Power Weight",
                OnModifySpd = (float spd, Pokemon source) =>
                {
                    spd = spd * 0.5f;
                    Debug.Log("Power Weight speed reduction");
                    return spd;
                },
                OnEVGain = (Pokemon source) =>
                {
                    var statEv = source.StatEffortValues;

                    statEv[Stat.HitPoints] = Mathf.Clamp((statEv[Stat.HitPoints] += 4), 0, GlobalSettings.i.MaxEvPerStat);

                    return statEv[Stat.HitPoints];
                }
            }
        },
        {
            HeldItemID.destiny_knot,
            new HeldItems()
            {
                Name = "Destiny Knot",
                OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                {
                    if (statusId != ConditionID.infatuation)
                        return true;

                    return false;
                }
            }
        },
        {
            HeldItemID.everstone,
            new HeldItems()
            {
                Name = "Everstone"
            }
        },

        #endregion

        #region Damaging Items
        {
            HeldItemID.black_sludge,
            new HeldItems()
            {
                Name = "Black Sludge",
                OnAfterTurn = (Pokemon pokemon, Pokemon other) =>
                {
                    if (pokemon.HasType(PokemonType.Poison))
                    {
                        Debug.Log("Healing with Black Sludge");
                        if (pokemon.HP < pokemon.MaxHp)
                            HealEnqueue(16, pokemon);
                    }
                    else
                    {
                        Debug.Log("Damaging with Black Sludge");
                        DamageEnqueue(8, pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.life_orb,
            new HeldItems()
            {
                Name = "Life Orb",
                OnModifyAtk = (float atk, Pokemon source, Pokemon target, Move move) =>
                {
                    atk = atk * 1.5f;
                    return atk;
                },
                OnAfterTurn = (Pokemon pokemon, Pokemon other) => DamageEnqueue(10, pokemon)
            }
        },
        {
            HeldItemID.rocky_helmet,
            new HeldItems()
            {
                Name = "Rocky Helmet",
                OnDamagingHit = (Pokemon defender, Pokemon attacker, Move move) =>
                {
                    if (attacker.CurrentMove.Base.HasFlag(MoveFlag.Contact))
                        DamageEnqueue(6, attacker);
                }
            }
        },
        {
            HeldItemID.sticky_barb,
            new HeldItems()
            {
                Name = "Sticky Barb",
                OnAfterTurn = (Pokemon source, Pokemon defender) => DamageEnqueue(8, source)
            }
        },
        #endregion
        
        {
            HeldItemID.grip_claw,
            new HeldItems()
            {
                Name = "Grip Claw",
                OnSetMove = (Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Binding))
                        return 8;
                    else
                        return 6;
                }
            }
        },
        {
            HeldItemID.venusaurite,
            new HeldItems()
            {
                Name = "Venusaurite"
            }
        },

        #region Berries
        // TODO: Lansat Berry, Micle Berry, Custap Berry

        {
            HeldItemID.aguav_berry,
            new HeldItems()
            {
                Name = "Aguav Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        HealEnqueue(3, pokemon);
                        NullHeldItem(pokemon);

                        List<string> validNatures = new List<string> { "NAUGHTY", "RASH", "LAX", "NAIVE" };
                        ConfusedNature(validNatures, pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.figy_berry,
            new HeldItems()
            {
                Name = "Figy Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        HealEnqueue(3, pokemon);
                        NullHeldItem(pokemon);

                        List<string> validNatures = new List<string> { "MODEST", "TIMID", "CALM", "BOLD" };
                        ConfusedNature(validNatures, pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.wiki_berry,
            new HeldItems()
            {
                Name = "Wiki Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        HealEnqueue(3, pokemon);
                        NullHeldItem(pokemon);

                        List<string> validNatures = new List<string> { "ADAMANT", "JOLLY", "CAREFUL", "IMPISH" };
                        ConfusedNature(validNatures, pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.mago_berry,
            new HeldItems()
            {
                Name = "Mago Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        HealEnqueue(3, pokemon);
                        NullHeldItem(pokemon);

                        List<string> validNatures = new List<string> { "BRAVE", "QUIET", "SASSY", "RELAXED" };
                        ConfusedNature(validNatures, pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.lepapa_berry,
            new HeldItems()
            {
                Name = "Lepapa Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        HealEnqueue(3, pokemon);
                        NullHeldItem(pokemon);

                        List<string> validNatures = new List<string> { "LONELY", "MILD", "GENTLE", "HASTY" };
                        ConfusedNature(validNatures, pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.cheri_berry,
            new HeldItems()
            {
                Name = "Cheri Berry",
                OnStatusChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.Status.Id == ConditionID.paralysis)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralysis was cured by its Cheri Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.chesto_berry,
            new HeldItems()
            {
                Name = "Chesto Berry",
                OnStatusChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.Status.Id == ConditionID.sleep)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s sleep was cured by its Chesto Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.pecha_berry,
            new HeldItems()
            {
                Name = "Pecha Berry",
                OnStatusChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.Status.Id == ConditionID.poison)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s poison was cured by its Pecha Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.rawst_berry,
            new HeldItems()
            {
                Name = "Rawst Berry",
                OnStatusChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.Status.Id == ConditionID.poison)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s burn was cured by its Rawst Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.aspear_berry,
            new HeldItems()
            {
                Name = "Aspear Berry",
                OnStatusChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.Status.Id == ConditionID.poison)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s freeze was cured by its Aspear Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.persim_berry,
            new HeldItems()
            {
                Name = "Persim Berry",
                OnStatusChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.Status.Id == ConditionID.poison)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s confusion was cured by its Persim Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.lum_berry,
            new HeldItems()
            {
                Name = "Lum Berry",
                OnStatusChanged = (Pokemon pokemon) =>
                {
                    bool cureableStatus = false;
                    string statusName = "";

                    switch (pokemon.Status.Id)
                    {
                        case ConditionID.poison:
                            statusName = "poison";
                            cureableStatus = true;
                            break;
                        case ConditionID.burn:
                            statusName = "burn";
                            cureableStatus = true;
                            break;
                        case ConditionID.sleep:
                            statusName = "sleep";
                            cureableStatus = true;
                            break;
                        case ConditionID.paralysis:
                            statusName = "paralysis";
                            cureableStatus = true;
                            break;
                        case ConditionID.freeze:
                            statusName = "freeze";
                            cureableStatus = true;
                            break;
                        case ConditionID.confusion:
                            statusName = "confusion";
                            cureableStatus = true;
                            break;
                        default:
                            break;
                    }

                    if (cureableStatus)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s {statusName} was cured by its Lum Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.sitrus_berry,
            new HeldItems()
            {
                Name = "Sitrus Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 2)
                    {
                        HealEnqueue(4, pokemon);
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.oran_berry,
            new HeldItems()
            {
                Name = "Oran Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 2)
                    {
                        pokemon.IncreaseHP(10);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was healed by its Oran Berry.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.leppa_berry,
            new HeldItems()
            {
                Name = "Leppa Berry",
                OnAfterMove = (Pokemon pokemon) =>
                {
                    if (pokemon.CurrentMove.PP <= 0)
                    {
                        pokemon.CurrentMove.IncreasePP(10);

                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s {pokemon.CurrentMove.Base.Name} PP was restored by 10.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.occa_berry,
            new HeldItems()
            {
                Name = "Occa Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Fire, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.passho_berry,
            new HeldItems()
            {
                Name = "Passho Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Water, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.wacan_berry,
            new HeldItems()
            {
                Name = "Wacan Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Electric, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.rindo_berry,
            new HeldItems()
            {
                Name = "Rindo Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Grass, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.yache_berry,
            new HeldItems()
            {
                Name = "Yache Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Ice, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.chople_berry,
            new HeldItems()
            {
                Name = "Chople Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Fighting, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.kebia_berry,
            new HeldItems()
            {
                Name = "Kebia Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Poison, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.shuca_berry,
            new HeldItems()
            {
                Name = "Shuca Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Ground, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.coba_berry,
            new HeldItems()
            {
                Name = "Coba Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Flying, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.payapa_berry,
            new HeldItems()
            {
                Name = "Payapa Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Psychic, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.tanga_berry,
            new HeldItems()
            {
                Name = "Tanga Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Bug, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.charti_berry,
            new HeldItems()
            {
                Name = "Charti Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Rock, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.kasib_berry,
            new HeldItems()
            {
                Name = "Kasib Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Ghost, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.haban_berry,
            new HeldItems()
            {
                Name = "Haban Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Dragon, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.colbur_berry,
            new HeldItems()
            {
                Name = "Colbur Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Dark, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.babiri_berry,
            new HeldItems()
            {
                Name = "Babiri Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Steel, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.chilan_berry,
            new HeldItems()
            {
                Name = "Chilan Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Normal, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.roseli_berry,
            new HeldItems()
            {
                Name = "Roseli Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    return SuperEffectiveEnqueue(move, PokemonType.Fairy, pokemon, damage);
                }
            }
        },
        {
            HeldItemID.enigma_berry,
            new HeldItems()
            {
                Name = "Enigma Berry",
                OnTakeDamage = (int damage, Pokemon pokemon, Move move) =>
                {
                    if (pokemon.TypeEffectiveness(move) == 1.5f)
                    {
                        damage = damage / 2;
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s Enigma Berry reduced {move.Base.Name} damage.");
                        NullHeldItem(pokemon);
                    }

                    return damage;
                }
            }
        },
        {
            HeldItemID.liechi_berry,
            new HeldItems()
            {
                Name = "Liechi Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        StatBoostEnqueue(Stat.Attack, pokemon);
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.ganlon_berry,
            new HeldItems()
            {
                Name = "Ganlon Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        StatBoostEnqueue(Stat.Defense, pokemon);
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.salac_berry,
            new HeldItems()
            {
                Name = "Salac Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        StatBoostEnqueue(Stat.Speed, pokemon);
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.petaya_berry,
            new HeldItems()
            {
                Name = "Petaya Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        StatBoostEnqueue(Stat.Special_attack, pokemon);
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.apicot_berry,
            new HeldItems()
            {
                Name = "Apicot Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        StatBoostEnqueue(Stat.Special_defense, pokemon);
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.starf_berry,
            new HeldItems()
            {
                Name = "Starf Berry",
                OnHpChanged = (Pokemon pokemon) =>
                {
                    Stat stat = Stat.Attack;
                    string statId = "";

                    int r = Random.Range(1, 6);

                    switch (r)
                    {
                        case 1:
                            stat = Stat.Attack;
                            statId = "attack";
                            break;
                        case 2:
                            stat = Stat.Defense;
                            statId = "defense";
                            break;
                        case 3:
                            stat = Stat.Special_attack;
                            statId = "special attack";
                            break;
                        case 4:
                            stat = Stat.Special_defense;
                            statId = "special defense";
                            break;
                        case 5:
                            stat = Stat.Speed;
                            statId = "speed";
                            break;
                        default:
                            break;
                    }

                    if (pokemon.HP <= pokemon.MaxHp / 4)
                    {
                        pokemon.StatBoosts[stat] += 2;
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s Starf Berry sharply raised its {statId}.");
                        NullHeldItem(pokemon);
                    }
                }
            }
        },
        {
            HeldItemID.kee_berry,
            new HeldItems()
            {
                Name = "Kee Berry",
                OnDamagingHit = (Pokemon defender, Pokemon attacker, Move move) =>
                {
                    if (move.Base.Category == MoveCategory.Physical)
                    {
                        StatBoostEnqueue(Stat.Defense, defender);
                        NullHeldItem(defender);
                    }
                }
            }
        },
        {
            HeldItemID.maranga_berry,
            new HeldItems()
            {
                Name = "Maranga Berry",
                OnDamagingHit = (Pokemon defender, Pokemon attacker, Move move) =>
                {
                    if (move.Base.Category == MoveCategory.Special)
                    {
                        StatBoostEnqueue(Stat.Special_defense, defender);
                        NullHeldItem(defender);
                    }
                }
            }
        },
        {
            HeldItemID.jaboca_berry,
            new HeldItems()
            {
                Name = "Jaboca Berry",
                OnDamagingHit = (Pokemon defender, Pokemon attacker, Move move) =>
                {
                    if (move.Base.Category == MoveCategory.Physical)
                    {
                        DamageEnqueue(8, attacker, defender);
                        NullHeldItem(defender);
                    }
                }
            }
        },
        {
            HeldItemID.rowap_berry,
            new HeldItems()
            {
                Name = "Rowap Berry",
                OnDamagingHit = (Pokemon defender, Pokemon attacker, Move move) =>
                {
                    if (move.Base.Category == MoveCategory.Physical)
                    {
                        DamageEnqueue(8, attacker, defender);
                        NullHeldItem(defender);
                    }
                }
            }
        },

    #endregion

    };

    private static void DamageEnqueue(int damage, Pokemon pokemon, Pokemon defender=null)
    {
        pokemon.DecreaseHP(pokemon.MaxHp / damage);

        if (defender == null)
            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by its {pokemon.HeldItems.Name}.");
        else
            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by {defender.HeldItems.Name}'s {defender.HeldItems.Name}.");
    }

    private static void HealEnqueue(int heal, Pokemon pokemon)
    {
        pokemon.IncreaseHP(pokemon.MaxHp / heal);
        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was healed by its {pokemon.HeldItems.Name}.");
    }

    private static void StatBoostEnqueue(Stat stat, Pokemon pokemon)
    {
        pokemon.StatBoosts[stat]++;
        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s {pokemon.HeldItems.Name} raised its {stat}.");
    }

    private static int SuperEffectiveEnqueue(Move move, PokemonType type, Pokemon pokemon, int damage)
    {
        if (pokemon.TypeEffectiveness(move) == 1.5f && move.Base.Type == type)
        {
            damage = damage / 2;
            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s {pokemon.HeldItems.Name} reduced {move.Base.Name} damage.");
            NullHeldItem(pokemon);
        }

        return damage;
    }

    private static void NullHeldItem(Pokemon pokemon)
    {
        pokemon.HeldItem = null;
        pokemon.HeldItems = HeldItems[HeldItemID.none];
    }

    private static void ConfusedNature(List<string> validNatures, Pokemon pokemon)
    {
        foreach (string nature in validNatures)
        {
            if (pokemon.Nature.ToUpper() == nature.ToUpper())
            {
                pokemon.SetVolatileStatus(pokemon, ConditionID.confusion);
                return;
            }
        }
    }
}

public enum HeldItemID
{
    none, black_sludge, choice_band, choice_scarf, choice_specs, life_orb, lucky_egg, macho_brace, power_anklet, power_band, 
    power_belt, power_bracer, power_lens, power_weight, aguav_berry, rocky_helmet, sticky_barb, cheri_berry, chesto_berry,
    pecha_berry, rawst_berry, aspear_berry, persim_berry, lum_berry, sitrus_berry, leppa_berry, oran_berry, figy_berry,
    wiki_berry, mago_berry, lepapa_berry, occa_berry, passho_berry, wacan_berry, rindo_berry, yache_berry, chople_berry,
    kebia_berry, shuca_berry, coba_berry, payapa_berry, tanga_berry, charti_berry, kasib_berry, haban_berry, colbur_berry,
    babiri_berry, chilan_berry, roseli_berry, liechi_berry, ganlon_berry, salac_berry, petaya_berry, apicot_berry, enigma_berry,
    starf_berry, kee_berry, maranga_berry, jaboca_berry, rowap_berry, grip_claw, destiny_knot, everstone,

    venusaurite
}
