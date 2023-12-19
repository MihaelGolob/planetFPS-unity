using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoostrapperInitializationComponent : MonoBehaviour {
    [SerializeField]
    private List<GameObject> managers;
    
    [SerializeField]
    private string sceneName;
    
    public void Awake() {
        // instantiate managers
        foreach (var manager in managers) {
            Instantiate(manager);
        }
        
        // change scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
