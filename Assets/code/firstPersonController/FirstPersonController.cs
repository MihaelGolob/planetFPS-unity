using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour, IDamageable {
    // inspector assigned
    [Header("Game objects")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private GameObject bodyObject;
    [SerializeField] private GameObject cameraObject;

    [Header("Mouse parameters")]
    [SerializeField] private float mouseSensitivity = 5.0f;

    [Header("Movement parameters")] 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private Vector2 upDownAngleRotation = new (-85f, 75f);
    [SerializeField] private float jumpHeight = 5;
    
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
    private Rigidbody _rigidbody;

    private bool _isGrounded;
    private float _gravitySpeed;
    private float _lastJumpTime;

    private bool _canZipline;
    private Collider _ziplineEnterCollider;
    private Zipline _activeZipline;
    private bool _isZiplining;
    
    // key tracking
    private Dictionary<KeyCode, short> _keysDictionary = new();
    private List<KeyCode> _keysToTrack = new() {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space, KeyCode.E};
    private Vector3 _lastMousePosition;
    private float _cameraAngle;

    private void Start() {
        _rootTransform = rootObject.transform;
        _bodyTransform = bodyObject.transform;
        _cameraTransform = cameraObject.transform;
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    public void Update() {
        UpdateKeys();
        UpdateRotation();
        UpdateMovement();
        Shoot();
        Zipline();
    }

    private void UpdateRotation() {
        Vector3 mouseDeltaPosition = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;

        // y axis
        _cameraAngle += mouseDeltaPosition.y;
        _cameraAngle = Mathf.Clamp(_cameraAngle, upDownAngleRotation.x, upDownAngleRotation.y);
        _cameraTransform.localRotation = Quaternion.Euler(-_cameraAngle, 0, 0);
        // x axis
        _bodyTransform.localRotation *= Quaternion.Euler(0, mouseDeltaPosition.x, 0);
    }

    private void UpdateMovement() {
        if (_isZiplining) return;

        Vector3 moveDir =
            _bodyTransform.forward * (_keysDictionary[KeyCode.W] - _keysDictionary[KeyCode.S]) +
            _bodyTransform.right * (_keysDictionary[KeyCode.D] - _keysDictionary[KeyCode.A]);

        if (moveDir.magnitude != 0)
            moveDir = moveDir.normalized;

        _isGrounded = isGroundedComponent.isGrounded;

        var speed = _isGrounded ? moveSpeed : airSpeed;
        moveDir *= speed * Time.deltaTime;

        _rigidbody.velocity = moveDir;

        // apply gravitational rotation
        var gravityDir = (gravitySource.position - _rootTransform.position).normalized;
        var bodyDownDir = -_rootTransform.up.normalized;
        var gravityRotation = Quaternion.FromToRotation(bodyDownDir, gravityDir);
        _rootTransform.rotation = gravityRotation * _rootTransform.rotation;

        // apply gravity
        if (_isGrounded && _gravitySpeed <= 0)
        {
            _gravitySpeed = 0;

            if (_keysDictionary[KeyCode.Space] > 0)
            {
                _gravitySpeed = jumpHeight;
            }
        }
        else
        {
            _gravitySpeed += -gravityAcceleration * Time.deltaTime;
        }

        var gravityVector = gravityDir * (-_gravitySpeed * Time.deltaTime);
        _rootTransform.position += gravityVector; //let it be 4 now :D
    }

    private void Shoot() {
        if (Input.GetMouseButton(0)) {
            weapon.Shoot(_cameraTransform.forward, gravitySource.position);
        }
    }

    private void Zipline() {
        if (!_canZipline || _isZiplining) return;
        
        if (Input.GetKeyDown(KeyCode.E)) {
            _isZiplining = true;
            var info = _activeZipline.GetZiplineInfo(_ziplineEnterCollider);
            gravitySource = info.new_planet;
            StartCoroutine(ZiplineInterpolation((info.start, info.end, info.speed)));
        }
    }

    private IEnumerator ZiplineInterpolation((Vector3 start, Vector3 end, float speed) info) {
        var t = 0f;
        while (t < 1f) {
            t += Time.deltaTime * info.speed;
            _rootTransform.position = Vector3.Lerp(info.start, info.end, t);
            yield return null;
        }
        
        _isZiplining = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Zipline")) {
            _canZipline = true;
            _ziplineEnterCollider = other;
            _activeZipline = other.transform.parent.parent.GetComponent<Zipline>();
        }
    }
    
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Zipline")) {
            _canZipline = false;
            _ziplineEnterCollider = null;
            _activeZipline = null;
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