using UnityEngine;

public abstract class WeaponBase : MonoBehaviour {
    [Header("basic")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    
    [Header("Weapon parameters")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletLifetime;
    [SerializeField] private float shootingFrequency;
    [SerializeField] private float bulletDamage;
    
    // private variables
    private float _lastShootTime;
    private float _shootCooldown;
    
    // private components
    private Animator _animator;
    
    // animator hashed parameters
    private readonly int _shootParameter = Animator.StringToHash("Shoot");

    private void Start() {
        _shootCooldown = 1 / shootingFrequency;
        _animator = GetComponent<Animator>();
    }

    public virtual void Shoot(Vector3 direction, Vector3 gravitySource) {
        if (Time.time - _lastShootTime < _shootCooldown) return;
        _lastShootTime = Time.time;
        _animator.SetTrigger(_shootParameter);
        
        var bulletObject = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        var bullet = bulletObject.GetComponent<Bullet>();
        bullet.Init(direction, gravitySource, bulletSpeed, bulletDamage, bulletLifetime);
    }
}
