using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Zipline))]
[CanEditMultipleObjects]
public class ZiplineEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Zipline myScript = (Zipline)target;
        if (GUILayout.Button("Create Cable Mesh")) {
            myScript.CreateCableMesh();
        }

        if (GUILayout.Button("Clear")) {
            myScript.Clear();
        }
    }
}
