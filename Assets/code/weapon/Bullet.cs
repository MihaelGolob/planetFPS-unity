using System;
using UnityEngine;

public class Bullet : MonoBehaviour {
    // parameters
    private Vector3 _gravitySource;
    private float _speed;
    private float _lifetime;
    private float _damage;
    
    // private variables
    private bool _isInitialized;
    private readonly float _gravityFactor = 15f;
    private Vector3 _gravityVelocity;

    private Rigidbody _rb;

    public void Init(Vector3 direction, Vector3 gravitySource, float speed, float damage, float lifetime) {
        _gravitySource = gravitySource;
        _speed = speed;
        _lifetime = lifetime;
        _damage = damage;
        
        _isInitialized = true;
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = direction * speed;
    }

    private void FixedUpdate() {
        if (!_isInitialized) return;
        
        var gravityVector = _gravityFactor * (_gravitySource - transform.position).normalized;
        // _gravityVelocity += gravityVector;
        _rb.AddForce(gravityVector, ForceMode.Acceleration);
    }

    private void Update() {
        if (!_isInitialized) return;
        
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0) {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if (!_isInitialized) return;
        
        var damageable = other.GetComponent<IDamageable>();
        damageable?.TakeDamage(_damage);
        
        Destroy(gameObject);
    }
}
