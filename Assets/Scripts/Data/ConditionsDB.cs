using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon, Pokemon user) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by poison.");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon, Pokemon user) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by its burn.");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.HasType(PokemonType.Electric))
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is immune to paralysis.");
                        return true;
                    }

                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralyzed and can't move.");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Frozen",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer frozen.");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Pokemon target, Pokemon source) =>
                {
                    // Sleep for 1-3 turns
                    target.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be asleep for {target.StatusTime} turns.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping.");
                    return false;
                }
            }
        },

        // Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has become confused",
                OnStart = (Pokemon target, Pokemon source) =>
                {
                    // Confused for 1-4 turns
                    target.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {target.VolatileStatusTime} turns.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} regained its composure!");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;

                    // 50% to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    // Hurt by confusion
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused.");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"It hurt itself in its confusion");
                    return false;
                }
            }
        },
        {
            ConditionID.seeded,
            new Condition()
            {
                Name = "Seeded",
                StartMessage = "was seeded!",
                OnAfterTurn = (Pokemon pokemon, Pokemon user) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);

                    if (user.HP != user.MaxHp)
                    {
                        user.IncreaseHP(pokemon.MaxHp / 8);
                    }

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s health is sapped by Leech Seed.");
                }
            }
        },
        {
            ConditionID.ability_change,
            new Condition()
            {
                Name = "Ability Change",
                OnAfterMove = (Pokemon pokemon) =>
                {
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s ability was swapped to {pokemon.Ability.Name}.");
                }
            }
        },
        {
            ConditionID.focus_energy,
            new Condition()
            {
                Name = "Focus Energy",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    var preFocus = pokemon.CurrentMove.Base.CritBehavior;
                    pokemon.CurrentMove.Base.CritBehavior = CritBehavior.HighCritRatio;
                    return true;
                }
            }
        },
        {
            ConditionID.sand_tomb,
            new Condition()
            {
                Name = "Sand Tomb",
                StartMessage = "was trapped by quick sand!",
                OnStart = (Pokemon target, Pokemon source) =>
                {
                    // Entombed for 4-5 turns
                    target.StatusTime = Random.Range(4, 6);
                    Debug.Log($"Will be tombed for {target.StatusTime} turns.");
                },
                OnAfterTurn = (Pokemon target, Pokemon source) =>
                {
                    if (target.StatusTime <= 0)
                    {
                        target.DecreaseHP(target.MaxHp / 8);
                        target.CureStatus();
                        target.StatusChanges.Enqueue($"{target.Base.Name} broke free of the quick sand!");
                    }

                    target.DecreaseHP(target.MaxHp / 16);
                    target.StatusTime--;
                    AudioManager.i.PlaySfx(AudioID.SandTombDMG);
                    target.StatusChanges.Enqueue($"{target.Base.Name} was hurt by the quick sand.");
                }
            }
        },
        {
            ConditionID.wrap,
            new Condition()
            {
                Name = "Wrap",
                StartMessage = "was wrapped!",
                OnStart = (Pokemon target, Pokemon source) =>
                {
                    // Entombed for 4-5 turns
                    int range = target.HeldItems?.OnSetMove?.Invoke(target.CurrentMove) ?? 6;

                    target.VolatileStatusTime = Random.Range(4, range);

                    Debug.Log($"Will be wrapped for {target.VolatileStatusTime} turns.");
                },
                OnAfterTurn = (Pokemon target, Pokemon source) =>
                {
                    if (target.VolatileStatusTime <= 0)
                    {
                        target.DecreaseHP(target.MaxHp / 8);
                        target.CureVolatileStatus();
                        target.StatusChanges.Enqueue($"{target.Base.Name} broke free of the quick sand!");
                    }

                    target.DecreaseHP(target.MaxHp / 16);
                    target.VolatileStatusTime--;
                    AudioManager.i.PlaySfx(AudioID.SandTombDMG);
                    target.StatusChanges.Enqueue($"{target.Base.Name} was hurt by the quick sand.");
                }
            }
        },
        {
            ConditionID.trapped,
            new Condition()
            {
                Name = "Trapped",
                StartMessage = "is trapped!",
                OnStart = (Pokemon target, Pokemon source) =>
                {
                    Debug.Log($"Will be trapped for {target.VolatileStatusTime} turns.");
                }
            }
        },
        {
            ConditionID.infatuation,
            new Condition()
            {
                Name = "Infatuated",
                StartMessage = "fell in love!",
                OnStart = (Pokemon target, Pokemon source) =>
                {
                    // Infatuated for 1-4 turns
                    target.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be infatuated for {target.VolatileStatusTime} turns.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} regained its composure!");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;

                    // 50% to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    // Failed attack
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is in love.");
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} doesn't want to attack!");
                    return false;
                }
            }
        },
        {
            ConditionID.flinch,
            new Condition()
            {
                Name = "Flinch",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} flinched and cannot move.");
                    pokemon.CureVolatileStatus();
                    return false;
                }
            }
        },

        // Weather Conditions
        {
            ConditionID.rain,
            new Condition()
            {
                Name = "Rain",
                StartMessage = "It has started to rain.",
                EffectMessage = "The rain continues to fall.",
                OnDamageModify = (Pokemon source, Pokemon target, Move move) =>
                {
                    if (source.Ability.Id != AbilityID.cloud_nine || target.Ability.Id != AbilityID.cloud_nine)
                    {
                        if (move.Base.Type == PokemonType.Water)
                            return 1.5f;
                        else if (move.Base.Type == PokemonType.Fire)
                            return 0.5f;
                    }

                    return 1f;
                }
            }
        },
        {
            ConditionID.sunny,
            new Condition()
            {
                Name = "Harsh Sunlight",
                StartMessage = "The weather has changed to Harsh Sunlight.",
                EffectMessage = "The sunlight is harsh",
                OnDamageModify = (Pokemon source, Pokemon target, Move move) =>
                {
                    if (source.Ability.Id != AbilityID.cloud_nine || target.Ability.Id != AbilityID.cloud_nine)
                    {
                        if (move.Base.Type == PokemonType.Fire)
                            return 1.5f;
                        else if (move.Base.Type == PokemonType.Water)
                            return 0.5f;
                    }

                    return 1f;
                }
            }
        },
        {
            ConditionID.sandstorm,
            new Condition()
            {
                Name = "Sandstorm",
                StartMessage = "A sandstorm is raging.",
                EffectMessage = "The sandstorm rages.",
                OnWeather = (Pokemon source, Pokemon target) =>
                {
                    if (source.Ability.Id != AbilityID.cloud_nine || target.Ability.Id != AbilityID.cloud_nine)
                    {
                        source.DecreaseHP(Mathf.RoundToInt((float)source.MaxHp / 16f));
                        source.StatusChanges.Enqueue($"{source.Base.Name} has been buffeted by the sandstorm.");
                    }
                }
            }
        },
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion, seeded, substitute, ability_change, sand_tomb, focus_energy, wrap,
    sunny, rain, sandstorm, trapped, infatuation, flinch, taunt
}
