using System.Collections;
using Ineor.Utils.AudioSystem;
using UnityEngine;
using UnityEngine.VFX;

public abstract class WeaponBase : MonoBehaviour {
    [Header("basic")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform bulletSpawnPoint;
    
    [Header("Weapon parameters")]
    [SerializeField] protected float initialVelocityFactor = 0.6f;
    [SerializeField] protected float bulletSpeed;
    [SerializeField] protected float bulletLifetime;
    [SerializeField] protected float shootingFrequency;
    [SerializeField] protected int bulletDamage;
    [SerializeField] protected int magazineSize;
    [SerializeField] protected float reloadTime;
    
    [Header("Effects")]
    [SerializeField] protected VisualEffect muzzleFlash;
    [SerializeField] protected Light muzzleLight;
    [SerializeField] protected float lightDuration = 0.1f;
    
    [Header("Audio collections")]
    [SerializeField] protected AudioCollection shootAudioCollection;
    [SerializeField] protected AudioCollection emptyMagazineAudioCollection;
    [SerializeField] protected AudioCollection reloadAudioCollection;
    
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

    //Networking
    private NetworkManager _network_manager;

    private void Start() {
        _shootCooldown = 1 / shootingFrequency;
        _animator = GetComponent<Animator>();
        _bulletsLeft = magazineSize;

        _network_manager = NetworkManager.game_object.GetComponent<NetworkManager>();
    }

    //Tule sem zacasno locu kreiranje metka, da lahko to poklicem preko networka. Zankrat deluje tako, da ko dobimo net msg, da je treba ustvarit metek,
    //si "sposodim" komponento od lokalnega igralca, da ustvarim metek. Amapak takrat mu ne sme znizati ammo, pregledovati rate of fire ... , zato
    //sem malo razbil tole funkcijo...
    //Popravi, ce mas cas, tj. Naredi neko staticno fcijo za generiranje metkov ali kaj takega...


    public virtual void CreateBullet(Vector3 pos, Vector3 velocity)
    {
        var bulletObject = Instantiate(bulletPrefab, pos, Quaternion.identity);
        var bullet = bulletObject.GetComponent<Bullet>();
        bullet.Init(velocity, bulletDamage, bulletLifetime);
    }

    public virtual void Shoot(Vector3 initial_velocity, Vector3 direction, Vector3 shootPosition) {
        if (Time.time - _lastShootTime < _shootCooldown) return;
        if (_reloadInProgress) return;
        
        if (_bulletsLeft == 0) {
            AudioSystem.Instance.PlaySound(emptyMagazineAudioCollection, shootPosition);
            _lastShootTime = Time.time;
            return;
        }
        
        _lastShootTime = Time.time;
        _animator.SetTrigger(_shootParameter);

        Vector3 velocity = initial_velocity * initialVelocityFactor + bulletSpeed * direction;

        CreateBullet(bulletSpawnPoint.position, velocity);
        _network_manager.tx_spawn_bullet(bulletSpawnPoint.position, velocity);

        _bulletsLeft = Mathf.Clamp(_bulletsLeft - 1, 0, magazineSize);
        // effects
        AudioSystem.Instance.PlaySound(shootAudioCollection, shootPosition);
        muzzleFlash.Play();
        StartCoroutine(ShowLight());
        HUDManager.Instance.UpdateAmmoCount(_bulletsLeft);
    }
    
    public void Reload() {
        if (_reloadInProgress) return;
        if (_bulletsLeft == magazineSize) return;
        
        StartCoroutine(ReloadInternal());
    }

    public void InstantReload() {
        _bulletsLeft = magazineSize;
        HUDManager.Instance.UpdateAmmoCount(_bulletsLeft);
    }

    private IEnumerator ReloadInternal() {
        AudioSystem.Instance.PlaySound(reloadAudioCollection, transform.position);
        _animator.SetTrigger(_gunDownParameter);
        _reloadInProgress = true;
        
        yield return new WaitForSeconds(reloadTime);
        
        _animator.SetTrigger(_gunUpParameter);
        _bulletsLeft = magazineSize;
        _reloadInProgress = false;
        HUDManager.Instance.UpdateAmmoCount(_bulletsLeft);
    }

    private IEnumerator ShowLight() {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleLight.enabled = false;
    }
}
