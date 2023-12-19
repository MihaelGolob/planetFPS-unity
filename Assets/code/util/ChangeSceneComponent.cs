using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneComponent : MonoBehaviour  {
    [SerializeField] 
    private string sceneName;

    public void Start() {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
