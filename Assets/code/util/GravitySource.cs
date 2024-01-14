using System;
using UnityEngine;

public class GravitySource : MonoBehaviour {
    private void OnEnable() {
        GravityManager.Instance.RegisterGravitySource(this);
    }
    
    private void OnDestroy() {
        GravityManager.Instance.UnregisterGravitySource(this);
    }

    private void OnDisable() {
        GravityManager.Instance.UnregisterGravitySource(this);
    }
}