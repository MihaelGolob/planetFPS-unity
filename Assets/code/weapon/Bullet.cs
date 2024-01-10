using System;
using UnityEngine;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour {
    [SerializeField] private VisualEffect bulletImpact;
    
    // parameters
    private float _lifetime;
    private int _damage;
    
    // private variables
    private bool _isInitialized;
    private readonly float _gravityFactor = 15f;
    private Vector3 _gravityVelocity;
    private Vector3 _gravitySource;

    private bool _collision;

    private Rigidbody _rb;
    private MeshRenderer _meshRenderer;

    public void Init(Vector3 velocity, int damage, float lifetime) {
        _lifetime = lifetime;
        _damage = damage;
        
        _isInitialized = true;
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = velocity;
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate() {
        if (!_isInitialized || _collision) return;
        
        _gravitySource = GravityManager.Instance.GetGravity(transform.position);
        var gravityVector = _gravityFactor * (_gravitySource - transform.position).normalized;
        // _gravityVelocity += gravityVector;
        _rb.AddForce(gravityVector, ForceMode.Acceleration);
    }

    void bulletExplode()
    {
        _collision = true;
        _rb.isKinematic = true;

        bulletImpact.Play();
        _meshRenderer.enabled = false;
        Destroy(gameObject, 3);
    }

    private void Update() {
        if (!_isInitialized || _collision) return;
        
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0) {
            bulletExplode();
        }
    }
    
    private void OnCollisionEnter(Collision other) {
        if (!_isInitialized || _collision) return;
        
        var damageable = other.gameObject.GetComponent<IDamageable>();
        damageable?.TakeDamage(_damage);

        bulletExplode();
    }
}
