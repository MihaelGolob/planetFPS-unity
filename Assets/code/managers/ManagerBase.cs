using System;
using UnityEngine;

public class ManagerBase : MonoBehaviour {
    protected virtual void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
