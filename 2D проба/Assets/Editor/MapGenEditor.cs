using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(mapGen))]
public class MapGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        mapGen mapGen = (mapGen)target;

        if (DrawDefaultInspector())
            if (mapGen.autoUpdate)
                mapGen.GenerateMap();

        if (GUILayout.Button("—генерировать"))
            mapGen.GenerateMap();
    }
}