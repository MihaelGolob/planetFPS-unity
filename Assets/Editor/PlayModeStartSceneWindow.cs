using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayModeStartSceneWindow : EditorWindow {
     void OnGUI() {
          // Use the Object Picker to select the start SceneAsset
          EditorSceneManager.playModeStartScene = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("Start Scene"), EditorSceneManager.playModeStartScene, typeof(SceneAsset), false);

          // Or set the start Scene from code
          var scenePath = "Assets/Scene3.unity";
          if (GUILayout.Button("Set start Scene: " + scenePath))
               SetPlayModeStartScene(scenePath);
     }

     void SetPlayModeStartScene(string scenePath) {
          SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
          if (myWantedStartScene != null)
               EditorSceneManager.playModeStartScene = myWantedStartScene;
          else
               Debug.Log("Could not find Scene " + scenePath);
     }

     [MenuItem("Custom/Set Play Mode Start Scene")]
     static void Open() {
          GetWindow<PlayModeStartSceneWindow>();
     }
}
