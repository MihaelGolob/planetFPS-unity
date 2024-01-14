using System;
using UnityEngine;
using UnityEngine.VFX;

public enum BulletType {
    Normal,
    Laser
}

public class Bullet : MonoBehaviour {
    [SerializeField] private VisualEffect bulletImpact;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private BulletType bulletType;
	[SerializeField] private float localBulletDeadTime = 0.2f;
    
    public BulletType BulletType => bulletType;
    
    // parameters
    private float _lifetime;
    private int _damage;
    
    // private variables
    private bool _isInitialized;
    private readonly float _gravityFactor = 15f;
    private Vector3 _gravityVelocity;
    private Vector3 _gravitySource;
    private bool _useGravity;
	private bool _networkBullet = false;
	private float _bulletCreationTime;

    private bool _collision;

    private Rigidbody _rb;

	public void Init(Vector3 velocity, int damage, float lifetime, bool useGravity = true, bool network_bullet=false) {
        _lifetime = lifetime;
        _damage = damage;
        _useGravity = useGravity;
		_networkBullet = network_bullet;
		_bulletCreationTime = Time.time;

        _isInitialized = true;
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = velocity;
    }

    private void FixedUpdate() {
        if (!_isInitialized || _collision) return;
        if (!_useGravity) return;
        
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
        meshRenderer.enabled = false;
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
        if (!_isInitialized || _collision)
			return; 
        
		if (!_networkBullet && (Time.time - _bulletCreationTime < localBulletDeadTime))
			return;

        var damageable = other.gameObject.GetComponent<IDamageable>();
        damageable?.TakeDamage(_damage);

        bulletExplode();
    }
}
