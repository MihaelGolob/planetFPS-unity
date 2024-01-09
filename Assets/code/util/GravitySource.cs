using System;
using UnityEngine;

public class GravitySource : MonoBehaviour {
    private void Start() {
        GravityManager.Instance.RegisterGravitySource(this);
    }
    
    private void OnDestroy() {
        GravityManager.Instance.UnregisterGravitySource(this);
    }
}