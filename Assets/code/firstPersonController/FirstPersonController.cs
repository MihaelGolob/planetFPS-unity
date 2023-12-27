using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Matrix4x4 = UnityEngine.Matrix4x4;

public class FirstPersonController : MonoBehaviour {
    // inspector assigned
    [Header("Game objects")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private GameObject bodyObject;
    [SerializeField] private GameObject cameraObject;

    [Header("Movement parameters")] 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float airSpeed;
    
    // private variables
    private Transform _rootTransform;
    private Transform _bodyTransform;
    private Transform _cameraTransform;

    private bool _isGrounded;
    
    // key tracking
    private Dictionary<KeyCode, short> _keysDictionary = new();
    private List<KeyCode> _keysToTrack = new() {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space};

    private void Start() {
        _rootTransform = rootObject.transform;
        _bodyTransform = bodyObject.transform;
        _cameraTransform = cameraObject.transform;
    }
    
    public void Update() {
        UpdateKeys();
        UpdateRotation();
        UpdateMovement();
    }

    private void UpdateRotation() {
    }

    private void UpdateMovement() {
       var moveDir = new Vector3(_keysDictionary[KeyCode.D] - _keysDictionary[KeyCode.A], 0, _keysDictionary[KeyCode.W] - _keysDictionary[KeyCode.S]);

       if (moveDir.magnitude != 0)
           moveDir = moveDir.normalized;

       var speed = _isGrounded ? moveSpeed : airSpeed;
       moveDir *= speed * Time.deltaTime;

       var globalBodyMatrix = Matrix4x4.TRS(_bodyTransform.position, _bodyTransform.rotation, _bodyTransform.localScale);
       var moveVector = globalBodyMatrix.MultiplyVector(moveDir);
       _rootTransform.position += moveVector;
    }

    private void UpdateKeys() {
        if (!_keysDictionary.ContainsKey(_keysToTrack[0])) {
            foreach (var key in _keysToTrack) {
                _keysDictionary.Add(key, 0);
            }
        }
        
        foreach(var key in _keysToTrack) {
            if (Input.GetKeyDown(key)) {
                _keysDictionary[key] = 1;
            } else if (Input.GetKeyUp(key)) {
                _keysDictionary[key] = 0;
            }
        }
    }

    private void TakeDamage(float damage) {
        
    }
}