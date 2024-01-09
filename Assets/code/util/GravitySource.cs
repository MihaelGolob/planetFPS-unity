using System;
using UnityEngine;

public class GravitySource : MonoBehaviour {

    [SerializeField] private int to_je_neki;
    
    private void Start() {
        GravityManager.Instance.RegisterGravitySource(this);
    }
    
    private void OnDestroy() {
        GravityManager.Instance.UnregisterGravitySource(this);
    }
}