using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorLock : MonoBehaviour {
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }
        
        if(Input.GetKey(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
        if(Input.GetButton("Fire1"))
            Cursor.lockState = CursorLockMode.Locked;
    }
}
