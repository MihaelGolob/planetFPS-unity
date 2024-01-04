using System;
using UnityEngine;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour {
    [SerializeField] private VisualEffect bulletImpact;
    
    // parameters
    private float _speed;
    private float _lifetime;
    private float _damage;
    
    // private variables
    private bool _isInitialized;
    private readonly float _gravityFactor = 15f;
    private Vector3 _gravityVelocity;
    private Vector3 _gravitySource;

    private bool _collision;

    private Rigidbody _rb;
    private MeshRenderer _meshRenderer;

    public void Init(Vector3 direction, float speed, float damage, float lifetime) {
        _speed = speed;
        _lifetime = lifetime;
        _damage = damage;
        
        _isInitialized = true;
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = direction * speed;
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate() {
        if (!_isInitialized || _collision) return;
        
        _gravitySource = GravityManager.Instance.GetGravity(transform.position);
        var gravityVector = _gravityFactor * (_gravitySource - transform.position).normalized;
        // _gravityVelocity += gravityVector;
        _rb.AddForce(gravityVector, ForceMode.Acceleration);
    }

    private void Update() {
        if (!_isInitialized || _collision) return;
        
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0) {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if (!_isInitialized || _collision) return;
        _collision = true;
        
        var damageable = other.GetComponent<IDamageable>();
        damageable?.TakeDamage(_damage);
        
        bulletImpact.Play();
        
        _meshRenderer.enabled = false;
        _rb.velocity = Vector3.zero;
        Destroy(gameObject, 4);
    }
}
