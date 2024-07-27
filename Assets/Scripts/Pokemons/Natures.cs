using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Natures
{
    private static NatureData[] natures = new NatureData[]
    {
        new NatureData("HARDY", 1, 1, 1, 1, 1),
        new NatureData("LONELY", 1.1f, 0.9f, 1, 1, 1),
        new NatureData("BRAVE", 1.1f, 1, 1, 1, 0.9f),
        new NatureData("ADAMANT", 1.1f, 1, 0.9f, 1, 1),
        new NatureData("NAUGHTY", 1.1f, 1, 1, 0.9f, 1),
        new NatureData("BOLD", 0.9f, 1.1f, 1, 1, 1),
        new NatureData("DOCILE", 1, 1, 1, 1, 1),
        new NatureData("RELAXED", 1, 1.1f, 1, 1, 0.9f),
        new NatureData("IMPISH", 1, 1.1f, 0.9f, 1, 1),
        new NatureData("LAX", 1, 1.1f, 1, 0.9f, 1),
        new NatureData("TIMID", 0.9f, 1, 1, 1, 1.1f),
        new NatureData("HASTY", 1, 0.9f, 1, 1, 1.1f),
        new NatureData("SERIOUS", 1, 1, 1, 1, 1),
        new NatureData("JOLLY", 1, 1, 0.9f, 1, 1.1f),
        new NatureData("NAIVE", 1, 1, 1, 0.9f, 1.1f),
        new NatureData("MODEST", 0.9f, 1, 1.1f, 1, 1),
        new NatureData("MILD", 1, 0.9f, 1.1f, 1, 1),
        new NatureData("QUIET", 1, 1, 1.1f, 1, 0.9f),
        new NatureData("BASHFUL", 1, 1, 1, 1, 1),
        new NatureData("RASH", 1, 1, 1.1f, 0.9f, 1),
        new NatureData("CALM", 0.9f, 1, 1, 1.1f, 1),
        new NatureData("GENTLE", 1, 0.9f, 1, 1.1f, 1),
        new NatureData("SASSY", 1, 1, 1, 1.1f, 0.9f),
        new NatureData("CAREFUL", 1, 1, 0.9f, 1.1f, 1),
        new NatureData("QUIRKY", 1, 1, 1, 1, 1)
    };

    public static NatureData GetNature(string name)
    {
        NatureData result = null;
        name = name.ToUpper();
        int i = 0;
        while (result == null)
        {
            if (i >= natures.Length)
                return null;
            else if (natures[i].GetName() == name)
                result = natures[i];

            i += 1;
        }

        return result;
    }

    public static NatureData RandomNature() => natures[Random.Range(0, natures.Length)];
}

public class NatureData
{
    private string name;
    private float[] Stat_mod = new float[5];

    public NatureData(string name, float ATK_MOD, float DEF_MOD, float SPA_MOD, float SPD_MOD, float SPE_MOD)
    {
        this.name = name;
        Stat_mod[0] = ATK_MOD;
        Stat_mod[1] = DEF_MOD;
        Stat_mod[2] = SPA_MOD;
        Stat_mod[3] = SPD_MOD;
        Stat_mod[4] = SPE_MOD;
    }

    public string GetName() => name;

    public float GetATK() => Stat_mod[0];

    public float GetDEF() => Stat_mod[1];

    public float GetSPA() => Stat_mod[2];

    public float GetSPD() => Stat_mod[3];

    public float GetSPE() => Stat_mod[4];
}
