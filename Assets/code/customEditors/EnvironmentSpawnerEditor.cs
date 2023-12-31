using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnvironmentSpawner))]
[CanEditMultipleObjects]
public class EnvironmentSpawnerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EnvironmentSpawner myScript = (EnvironmentSpawner)target;
        if (GUILayout.Button("Spawn")) {
            myScript.Spawn();
        }
        
        if (GUILayout.Button("Clear")) {
            myScript.Clear();
        }
    }
}
