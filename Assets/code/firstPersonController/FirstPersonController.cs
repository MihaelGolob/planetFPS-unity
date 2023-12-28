using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class FirstPersonController : MonoBehaviour, IDamageable {
    // inspector assigned
    [Header("Game objects")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private GameObject bodyObject;
    [SerializeField] private GameObject cameraObject;

    [Header("Movement parameters")] 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private Vector2 upDownAngleRotation = new (-85f, 75f);
    [SerializeField] private float jumpHeight = 5;
    [SerializeField] private float jumpCooldown = 0.5f;
    
    [Header("Shooting")]
    [SerializeField] private WeaponBase weapon;

    [Header("Gravity")] 
    [SerializeField] private IsGroundedComponent isGroundedComponent;
    [SerializeField] private float gravityAcceleration = 9.8f;
    [SerializeField] private Transform gravitySource;
    
    // public members
    public float Health { get; private set; } = 100f;
    
    // private variables
    private Transform _rootTransform;
    private Transform _bodyTransform;
    private Transform _cameraTransform;

    private bool _isGrounded;
    private float _gravitySpeed;
    private float _lastJumpTime;
    
    // key tracking
    private Dictionary<KeyCode, short> _keysDictionary = new();
    private List<KeyCode> _keysToTrack = new() {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space};
    private Vector3 _lastMousePosition;
    private float _cameraAngle;

    private void Start() {
        _rootTransform = rootObject.transform;
        _bodyTransform = bodyObject.transform;
        _cameraTransform = cameraObject.transform;
    }
    
    public void Update() {
        UpdateKeys();
        UpdateRotation();
        UpdateMovement();
        Shoot();
    }

    private void UpdateRotation() {
        var mousePosition = Input.mousePosition;
        var mouseDeltaPosition = mousePosition - _lastMousePosition;
        _lastMousePosition = mousePosition;

        // y axis
        _cameraAngle += mouseDeltaPosition.y;
        _cameraAngle = Mathf.Clamp(_cameraAngle, upDownAngleRotation.x, upDownAngleRotation.y);
        _cameraTransform.localRotation = Quaternion.Euler(-_cameraAngle, 0, 0);
        // x axis
        _bodyTransform.localRotation *= Quaternion.Euler(0, mouseDeltaPosition.x, 0);
    }

    private void UpdateMovement() {
       var moveDir = new Vector3(_keysDictionary[KeyCode.D] - _keysDictionary[KeyCode.A], 0, _keysDictionary[KeyCode.W] - _keysDictionary[KeyCode.S]);

       if (moveDir.magnitude != 0)
           moveDir = moveDir.normalized;

       _isGrounded = isGroundedComponent.isGrounded;
       
       var speed = _isGrounded ? moveSpeed : airSpeed;
       moveDir *= speed * Time.deltaTime;

       var globalBodyMatrix = Matrix4x4.TRS(_bodyTransform.position, _bodyTransform.rotation, _bodyTransform.localScale);
       var moveVector = globalBodyMatrix.MultiplyVector(moveDir);
       // apply movement
       _rootTransform.position += moveVector;
       
       // apply gravitational rotation
       var gravityDir = (gravitySource.position - _rootTransform.position).normalized;
       var bodyDownDir = -_rootTransform.up.normalized;
       var gravityRotation = Quaternion.FromToRotation(bodyDownDir, gravityDir);
       _rootTransform.rotation = gravityRotation * _rootTransform.rotation;
       
       // apply gravity
       if (_isGrounded && _gravitySpeed <= 0) {
           _gravitySpeed = 0;
       
           if (_keysDictionary[KeyCode.Space] > 0 && Time.time - _lastJumpTime > jumpCooldown) {
               _gravitySpeed = jumpHeight;
               _lastJumpTime = Time.time;
           }
       } else {
           _gravitySpeed += -gravityAcceleration * Time.deltaTime;
       }
       
       var gravityVector = gravityDir * (-_gravitySpeed * Time.deltaTime);
       _rootTransform.position += gravityVector;
    }

    private void Shoot() {
        Debug.Log(_cameraTransform.forward);
        if (Input.GetMouseButton(0)) {
            weapon.Shoot(_cameraTransform.forward);
        }
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

    public void TakeDamage(float damage) {
        Health -= damage;
    }
}