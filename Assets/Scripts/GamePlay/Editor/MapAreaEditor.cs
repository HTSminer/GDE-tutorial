using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChanceInDayGrass = serializedObject.FindProperty("totalChanceDay").intValue;
        int totalChanceInNightGrass = serializedObject.FindProperty("totalChanceNight").intValue;
        int totalChanceInWater = serializedObject.FindProperty("totalChanceWater").intValue;

        if (totalChanceInDayGrass != 100 && totalChanceInDayGrass != -1)
            EditorGUILayout.HelpBox($"The total daytime chance percentage of Pokemon in grass is {totalChanceInDayGrass} and not 100", MessageType.Error);

        if (totalChanceInNightGrass != 100 && totalChanceInNightGrass != -1)
            EditorGUILayout.HelpBox($"The total Night time chance percentage of Pokemon in grass is {totalChanceInNightGrass} and not 100", MessageType.Error);

        if (totalChanceInWater != 100 && totalChanceInWater != -1)
            EditorGUILayout.HelpBox($"The total chance percentage of Pokemon in water is {totalChanceInWater} and not 100", MessageType.Error);
    }
}
