using System.Collections;
using Ineor.Utils.AudioSystem;
using UnityEngine;
using UnityEngine.VFX;

public abstract class WeaponBase : MonoBehaviour {
    [Header("basic")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    
    [Header("Weapon parameters")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletLifetime;
    [SerializeField] private float shootingFrequency;
    [SerializeField] private float bulletDamage;
    
    [Header("Effects")]
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private float lightDuration = 0.1f;
    [SerializeField] private AudioCollection shootAudioCollection;
    
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

    public virtual void Shoot(Vector3 direction) {
        if (Time.time - _lastShootTime < _shootCooldown) return;
        _lastShootTime = Time.time;
        _animator.SetTrigger(_shootParameter);
        
        var bulletObject = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        var bullet = bulletObject.GetComponent<Bullet>();
        bullet.Init(direction, bulletSpeed, bulletDamage, bulletLifetime);
        
        // effects
        AudioSystem.Instance.PlaySound(shootAudioCollection, transform.position);
        muzzleFlash.Play();
        StartCoroutine(ShowLight());
    }

    private IEnumerator ShowLight() {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleLight.enabled = false;
    }
}
