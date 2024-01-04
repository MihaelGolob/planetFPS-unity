using System;
using UnityEngine;

public class GravitySource : MonoBehaviour{
    private void Start() {
        GravityManager.Instance.RegisterGravitySource(this);
    }
}