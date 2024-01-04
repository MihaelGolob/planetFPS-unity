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
    [SerializeField] private int magazineSize;
    [SerializeField] private float reloadTime;
    
    [Header("Effects")]
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private float lightDuration = 0.1f;
    
    [Header("Audio collections")]
    [SerializeField] private AudioCollection shootAudioCollection;
    [SerializeField] private AudioCollection emptyMagazineAudioCollection;
    [SerializeField] private AudioCollection reloadAudioCollection;
    
    // private variables
    private float _lastShootTime;
    private float _shootCooldown;
    private int _bulletsLeft;
    private bool _reloadInProgress;
    
    // private components
    private Animator _animator;
    
    // animator hashed parameters
    private readonly int _shootParameter = Animator.StringToHash("Shoot");
    private readonly int _gunDownParameter = Animator.StringToHash("GunDown");
    private readonly int _gunUpParameter = Animator.StringToHash("GunUp");

    private void Start() {
        _shootCooldown = 1 / shootingFrequency;
        _animator = GetComponent<Animator>();
        _bulletsLeft = magazineSize;
    }

    public virtual void Shoot(Vector3 direction, Vector3 shootPosition) {
        if (Time.time - _lastShootTime < _shootCooldown) return;
        if (_reloadInProgress) return;
        
        if (_bulletsLeft == 0) {
            AudioSystem.Instance.PlaySound(emptyMagazineAudioCollection, shootPosition);
            _lastShootTime = Time.time;
            return;
        }
        
        _lastShootTime = Time.time;
        _animator.SetTrigger(_shootParameter);
        
        var bulletObject = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        var bullet = bulletObject.GetComponent<Bullet>();
        bullet.Init(direction, bulletSpeed, bulletDamage, bulletLifetime);

        _bulletsLeft = Mathf.Clamp(_bulletsLeft - 1, 0, magazineSize);
        // effects
        AudioSystem.Instance.PlaySound(shootAudioCollection, shootPosition);
        muzzleFlash.Play();
        StartCoroutine(ShowLight());
    }
    
    public void Reload() {
        if (_reloadInProgress) return;
        if (_bulletsLeft == magazineSize) return;
        
        StartCoroutine(ReloadInternal());
    }

    private IEnumerator ReloadInternal() {
        AudioSystem.Instance.PlaySound(reloadAudioCollection, transform.position);
        _animator.SetTrigger(_gunDownParameter);
        _reloadInProgress = true;
        
        yield return new WaitForSeconds(reloadTime);
        
        _animator.SetTrigger(_gunUpParameter);
        _bulletsLeft = magazineSize;
        _reloadInProgress = false;
    }

    private IEnumerator ShowLight() {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleLight.enabled = false;
    }
}
