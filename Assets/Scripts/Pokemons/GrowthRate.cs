#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a growth rate with associated experience values and formula.
/// </summary>
public class GrowthRate : MonoBehaviour
{
    /// <summary>
    /// Gets the unique identifier for this growth rate.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the name of this growth rate.
    /// </summary>
    public string RealName { get; private set; }

    /// <summary>
    /// Gets the list of experience values associated with this growth rate.
    /// </summary>
    public IReadOnlyList<int> ExpValues { get; private set; }

    /// <summary>
    /// Gets the formula used to calculate experience for levels beyond the predefined values.
    /// </summary>
    public Func<int, int>? ExpFormula { get; private set; }

    /// <summary>
    /// Used to store GrowthRates after being Registered.
    /// </summary>
    public static Dictionary<int, GrowthRate> Data = new Dictionary<int, GrowthRate>();

    static GrowthRate()
    {
        try
        {
            Register(Medium);
            Register(Erratic);
            Register(Fluctuating);
            Register(MediumFast);
            Register(Fast);
            Register(Slow);
        }
        catch (ArgumentException argEx)
        {
            Debug.LogError($"Argument error during GrowthRate initialization: {argEx.Message}");
        }
        catch (KeyNotFoundException keyEx)
        {
            Debug.LogError($"Key not found during GrowthRate initialization: {keyEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize GrowthRate class: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the name of this growth rate.
    /// </summary>
    /// <returns>(string) the translated name of this growth rate</returns>
    public string GetName() => RealName;

    //public static void Load() { /* Load data here */ }
    //public static void Save() { /* Save data here */ }

    /// <summary>
    /// Initializes a new instance of the GrowthRate class.
    /// </summary>
    public GrowthRate(int id, string name, List<int>? expValues, Func<int, int>? expFormula)
    {
        Id = id;
        RealName = name ?? "Unnamed";
        ExpValues = expValues?.AsReadOnly() ?? new List<int>().AsReadOnly();
        ExpFormula = expFormula;
    }

    /// <summary>
    /// Gets the maximum level a Pokémon can reach in this game.
    /// </summary>
    /// <returns>(int) the max level a Pokemon can reach in this game.</returns>
    public static int GetMaxLevel() => GlobalSettings.i.MaxLevel;

    /// <summary>
    /// Gets the minimum experience needed for a Pokémon to reach the given level.
    /// </summary>
    /// <param name="level">(int) a level number</param>
    /// <returns>(int) the minimum Exp needed to be at the given level</returns>
    public int MinimumExpForLevel(int level)
    {
        if (level <= 0 || level > GetMaxLevel()) 
            throw new ArgumentException($"Level {level} is invalid");

        if (level < ExpValues.Count) 
            return ExpValues[level];

        if (ExpFormula == null) 
            throw new Exception($"No Exp formula is defined for growth rate {GetName()}");

        return ExpFormula(level);
    }

    /// <summary>
    /// Gets the maximum experience a Pokémon with this GrowthRate can have.
    /// </summary>
    /// <returns>(int) the maximum Exp a Pokemon with this GrowthRate can have.</returns>
    public int MaximumExp() => MinimumExpForLevel(GetMaxLevel());

    /// <summary>
    /// Adds two experience amounts, clamping the result to the maximum experience.
    /// </summary>
    /// <param name="exp1">(int)an Exp amount</param>
    /// <param name="exp2">(int)an Exp amount</param>
    /// <returns>(int) the sum of the two given Exp amounts</returns>
    public int AddExp(int exp1, int exp2) => Math.Clamp(exp1 + exp2, 0, MaximumExp());

    /// <summary>
    /// Gets the level of a Pokémon with the given experience amount.
    /// </summary>
    /// <param name="exp">(int)an Exp amount</param>
    /// <returns>(int) the level of a Pokemon that has the given Exp amount</returns>
    public int LevelFromExp(int exp)
    {
        if (exp < 0) throw new ArgumentException($"Exp amount {exp} is invalid");
        int max = GetMaxLevel();
        if (exp >= MaximumExp()) return max;
        for (int level = 1; level <= max; level++)
        {
            if (exp < MinimumExpForLevel(level)) return level - 1;
        }
        return max;
    }

    /// <summary>
    /// Registers GrowthRates into the Dictionary.
    /// </summary>
    /// <param name="growthRate">(GrowthRate) a static growth rate</param>
    public static void Register(GrowthRate growthRate) => Data[growthRate.Id] = growthRate;

    /// <summary>
    /// Gets a GrowthRate by its registered ID.
    /// </summary>
    /// <param name="id">(GrowthRateID) an id enum selected in editor.</param>
    public static GrowthRate GetGrowthRate(GrowthRateID id)
    {
        if (Data.ContainsKey((int)id))
            return Data[(int)id];
        throw new KeyNotFoundException($"GrowthRate with id {(int)id} not found");
    }


    public static readonly GrowthRate Medium = new GrowthRate(
        0,
        "Medium",
        new List<int>
        {
            -1, 0, 9, 57, 96, 135, 179, 236, 314, 419, 560,
            742, 973, 1261, 1612, 2035, 2535, 3120, 3798, 4575, 5460,
            6458, 7577, 8825, 10208, 11735, 13411, 15244, 17242, 19411, 21760,
            24294, 27021, 29949, 33084, 36435, 40007, 43808, 47846, 52127, 56660,
            61450, 66505, 71833, 77440, 83335, 89523, 96012, 102810, 109923, 117360,
            125126, 133229, 141677, 150476, 159635, 169159, 179056, 189334, 199999, 211060,
            222522, 234393, 246681, 259392, 272535, 286115, 300140, 314618, 329555, 344960,
            360838, 377197, 394045, 411388, 429235, 447591, 466464, 485862, 505791, 526260,
            547274, 568841, 590969, 613664, 636935, 660787, 685228, 710266, 735907, 762160,
            789030, 816525, 844653, 873420, 902835, 932903, 963632, 995030, 1027103, 1059860
        },
        level => (int)Math.Pow(level, 3)
    );

    public static readonly GrowthRate Erratic = new GrowthRate(
        1,
        "Erratic",
        new List<int>
        {
            -1, 0, 9, 57, 96, 135, 179, 236, 314, 419, 560,
            742, 973, 1261, 1612, 2035, 2535, 3120, 3798, 4575, 5460,
            6458, 7577, 8825, 10208, 11735, 13411, 15244, 17242, 19411, 21760,
            24294, 27021, 29949, 33084, 36435, 40007, 43808, 47846, 52127, 56660,
            61450, 66505, 71833, 77440, 83335, 89523, 96012, 102810, 109923, 117360,
            125126, 133229, 141677, 150476, 159635, 169159, 179056, 189334, 199999, 211060,
            222522, 234393, 246681, 259392, 272535, 286115, 300140, 314618, 329555, 344960,
            360838, 377197, 394045, 411388, 429235, 447591, 466464, 485862, 505791, 526260,
            547274, 568841, 590969, 613664, 636935, 660787, 685228, 710266, 735907, 762160,
            789030, 816525, 844653, 873420, 902835, 932903, 963632, 995030, 1027103, 1059860
        },
        level => ((int)Math.Pow(level, 4) + (int)Math.Pow(level, 3) * 2000) / 3500
    );

    public static readonly GrowthRate Fluctuating = new GrowthRate(
        2,
        "Fluctuating",
        new List<int>
        {
            -1, 0, 9, 57, 96, 135, 179, 236, 314, 419, 560,
            742, 973, 1261, 1612, 2035, 2535, 3120, 3798, 4575, 5460,
            6458, 7577, 8825, 10208, 11735, 13411, 15244, 17242, 19411, 21760,
            24294, 27021, 29949, 33084, 36435, 40007, 43808, 47846, 52127, 56660,
            61450, 66505, 71833, 77440, 83335, 89523, 96012, 102810, 109923, 117360,
            125126, 133229, 141677, 150476, 159635, 169159, 179056, 189334, 199999, 211060,
            222522, 234393, 246681, 259392, 272535, 286115, 300140, 314618, 329555, 344960,
            360838, 377197, 394045, 411388, 429235, 447591, 466464, 485862, 505791, 526260,
            547274, 568841, 590969, 613664, 636935, 660787, 685228, 710266, 735907, 762160,
            789030, 816525, 844653, 873420, 902835, 932903, 963632, 995030, 1027103, 1059860
        },
        level => ((int)Math.Pow(level, 3) + (level / 2 + 32) * 4) / (100 + level)
    );

    public static readonly GrowthRate MediumFast = new GrowthRate(
        3,
        "MediumFast",
        new List<int>
        {
            -1, 0, 9, 57, 96, 135, 179, 236, 314, 419, // 1 - 10
            560, 742, 973, 1261, 1612, 2035, 2535, 3120, 3798, 4575, // 11 - 20
            5460, 6458, 7577, 8825, 10208, 11735, 13411, 15244, 17242, 19411, // 21 - 30
            21760, 24294, 27021, 29949, 33084, 36435, 40007, 43808, 47846, 52127, // 31 - 40
            56660, 61450, 66505, 71833, 77440, 83335, 89523, 96012, 102810, 109923, // 41 - 50
            117360, 125126, 133229, 141677, 150476, 159635, 169159, 179056, 189334, 199999, // 51 - 60
            211060, 222522, 234393, 246681, 259392, 272535, 286115, 300140, 314618, 329555, // 61 - 70
            344960, 360838, 377197, 394045, 411388, 429235, 447591, 466464, 485862, 505791, // 71 - 80
            526260, 547274, 568841, 590969, 613664, 636935, 660787, 685228, 710266, 735907, // 81 - 90
            762160, 789030, 816525, 844653, 873420, 902835, 932903, 963632, 995030, 1027103, 1059860 // 91 - 100
        },
        level => (int)Math.Pow(level, 3) + (int)Math.Pow(level, 2) * 6 / 5 - 15
    );

    public static readonly GrowthRate Fast = new GrowthRate(
        4,
        "Fast",
        new List<int>
        {
            -1, 0, 9, 57, 96, 135, 179, 236, 314, 419, 560,
            742, 973, 1261, 1612, 2035, 2535, 3120, 3798, 4575, 5460,
            6458, 7577, 8825, 10208, 11735, 13411, 15244, 17242, 19411, 21760,
            24294, 27021, 29949, 33084, 36435, 40007, 43808, 47846, 52127, 56660,
            61450, 66505, 71833, 77440, 83335, 89523, 96012, 102810, 109923, 117360,
            125126, 133229, 141677, 150476, 159635, 169159, 179056, 189334, 199999, 211060,
            222522, 234393, 246681, 259392, 272535, 286115, 300140, 314618, 329555, 344960,
            360838, 377197, 394045, 411388, 429235, 447591, 466464, 485862, 505791, 526260,
            547274, 568841, 590969, 613664, 636935, 660787, 685228, 710266, 735907, 762160,
            789030, 816525, 844653, 873420, 902835, 932903, 963632, 995030, 1027103, 1059860
        },
        level => (int)(4 * Math.Pow(level, 3) / 5)
    );

    public static readonly GrowthRate Slow = new GrowthRate(
        5,
        "Slow",
        new List<int>
        {
            -1, 0, 9, 57, 96, 135, 179, 236, 314, 419, 560,
            742, 973, 1261, 1612, 2035, 2535, 3120, 3798, 4575, 5460,
            6458, 7577, 8825, 10208, 11735, 13411, 15244, 17242, 19411, 21760,
            24294, 27021, 29949, 33084, 36435, 40007, 43808, 47846, 52127, 56660,
            61450, 66505, 71833, 77440, 83335, 89523, 96012, 102810, 109923, 117360,
            125126, 133229, 141677, 150476, 159635, 169159, 179056, 189334, 199999, 211060,
            222522, 234393, 246681, 259392, 272535, 286115, 300140, 314618, 329555, 344960,
            360838, 377197, 394045, 411388, 429235, 447591, 466464, 485862, 505791, 526260,
            547274, 568841, 590969, 613664, 636935, 660787, 685228, 710266, 735907, 762160,
            789030, 816525, 844653, 873420, 902835, 932903, 963632, 995030, 1027103, 1059860
        },
        level => (int)(5 * Math.Pow(level, 3) / 4)
    );
}

public enum GrowthRateID { erratic, medium_slow, fluctuating, medium, fast, slow }