using System.Collections;
using Ineor.Utils.AudioSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;
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
    [SerializeField] public float moveSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private Vector2 upDownAngleRotation = new Vector2(-90.0f, 90.0f);
    [SerializeField] public float jumpHeight = 5;
    
    [Header("Weapons")]
    [SerializeField] private WeaponBase assaultRifleWeapon;
    [SerializeField] private WeaponBase laserGunWeapon;
    [SerializeField] private float weaponSwitchTime = 1f;
    [SerializeField] private FullScreenDamageController damageEffectController;

    [Header("Gravity")] 
    [SerializeField] private float gravityAcceleration = 9.8f;
    
    [Header("Audio Collections")]
    [SerializeField] private AudioCollection hurtAudioCollection;
    [SerializeField] private AudioCollection deathAudioCollection;
    
    private Vector3 _gravitySource;
    
    // public members
    public int Health { get; set; } = 100;
    public bool IsMoving { get; set; } = false;
    
    // private variables
    private Transform _rootTransform;
    private Transform _bodyTransform;
    private Transform _cameraTransform;
    private Rigidbody _rigidbody;
    private NetworkManager _network_manager;
    
    private WeaponBase _activeWeapon;
    private bool _isSwitching;

	public bool isGrounded { private set; get; } = false;
    private float _gravitySpeed;
    private float _lastJumpTime;

    private bool _canZipline;
    private Collider _ziplineEnterCollider;
    private Zipline _activeZipline;
	public bool isDead { get; private set; } = false;
    public bool isZiplining { get; private set; } = false;
    
    private Vector3 _lastMousePosition;
    private float _cameraAngle;
    private Vector3 prev_pos = Vector3.zero;
    public Vector3 player_velocity { get; private set; } = Vector3.zero;

    private void Start() {
        _rootTransform = rootObject.transform;
        _bodyTransform = bodyObject.transform;
        _cameraTransform = cameraObject.transform;
        _rigidbody = GetComponent<Rigidbody>();
        _network_manager = NetworkManager.game_object.GetComponent<NetworkManager>();
        _network_manager.tx_spawn_player(_rootTransform.position); // To je sicer delo game Managerja ...
        
        _activeWeapon = assaultRifleWeapon;
        _activeWeapon.UpdateAmmoCount();
        
        HUDManager.Instance.UpdateHealth(Health);
    }
    
    private void CalculateVelocity() {
        player_velocity = (_cameraTransform.position - prev_pos) * (Time.deltaTime * 2000);
        prev_pos = _cameraTransform.position;
    }

    public void Update() {
        // input functions
        if (!HUDManager.Instance.IsPauseMenuEnabled) {
            UpdateRotation();
            UpdateMovement();
            Shoot();
            Reload();
            Zipline();
            SwitchWeaponInput();
        }
        CalculateVelocity();

        _network_manager.tx_move_player(_rootTransform.position, _bodyTransform.rotation);
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
        IsMoving = false;
        if (isZiplining) return;

        Vector3 moveDir =
            _bodyTransform.forward * Input.GetAxis("Vertical") +
            _bodyTransform.right * Input.GetAxis("Horizontal");

        if (moveDir.magnitude != 0) {
            moveDir = moveDir.normalized;
            IsMoving = true;
        }

        //isGrounded = isGroundedComponent.isGrounded;
		//isGrounded = true;
		RaycastHit hit;

		if(Physics.SphereCast(_rootTransform.position + _rootTransform.up * 1.25f, 0.25f, -_rootTransform.up, out hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {
			isGrounded = hit.distance <= 1.0f;
		}
			
        var speed = isGrounded ? moveSpeed : airSpeed;
        //moveDir *= speed * Time.deltaTime;
		moveDir *= speed; //Ne smemo mnozit z deltaTime, ker ze rigidbody sam to naredi...
	
        _rigidbody.velocity = moveDir;

        // set source
        _gravitySource = GravityManager.Instance.GetGravity(transform.position);

        // apply gravitational rotation
        var gravityDir = (_gravitySource - _rootTransform.position).normalized;
        var bodyDownDir = -_rootTransform.up.normalized;
        var gravityRotation = Quaternion.FromToRotation(bodyDownDir, gravityDir);
        _rootTransform.rotation = gravityRotation * _rootTransform.rotation;

        // apply gravity
        if (isGrounded && _gravitySpeed <= 0)
        {
            _gravitySpeed = 0;

            if (Input.GetKey(KeyCode.Space))
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
        if (Input.GetMouseButton(0) && !_isSwitching) {
            _activeWeapon.Shoot(player_velocity, _cameraTransform.forward, transform.position);
        }
    }
    
    private void Reload() {
        if (Input.GetKeyDown(KeyCode.R) && !_isSwitching) {
            _activeWeapon.Reload();
        }
    }

    private void SwitchWeaponInput() {
        if (Input.GetKeyDown(KeyCode.Q) && !_isSwitching) {
            StartCoroutine(SwitchWeapons());
        }
    }

    private void Zipline() {
        if (!_canZipline || isZiplining) return;
        
        if (Input.GetKeyDown(KeyCode.E)) {
            isZiplining = true;
            var info = _activeZipline.GetZiplineInfo(_ziplineEnterCollider);
			StartCoroutine(ZiplineInterpolation((info.start.position, info.end.position, info.speed), info.start.up, info.end.up));
        }
    }

	private IEnumerator ZiplineInterpolation((Vector3 start, Vector3 end, float speed) info, Vector3 pole1_up, Vector3 pole2_up) {
		Quaternion quat1 = _rootTransform.rotation;
		Quaternion quat2 = Quaternion.FromToRotation(pole1_up, pole2_up);

        var t = 0f;
        while (t < 1f) {
            t += Time.deltaTime * info.speed;
            _rootTransform.position = Vector3.Lerp(info.start, info.end, t);
			_rootTransform.rotation = Quaternion.Lerp(quat1, quat2, t);
            yield return null;
        }
        
        isZiplining = false;
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

    public IEnumerator SwitchWeapons() {
        _isSwitching = true;
        _activeWeapon.LowerWeapon();
        var otherWeapon = _activeWeapon == assaultRifleWeapon ? laserGunWeapon : assaultRifleWeapon;
        otherWeapon.LowerWeapon();
        
        yield return new WaitForSeconds(weaponSwitchTime);
        
        _activeWeapon.EnableMesh(false);
        _activeWeapon.RaiseWeapon();
        
        otherWeapon.EnableMesh(true);
        otherWeapon.UpdateAmmoCount();
        otherWeapon.RaiseWeapon();
        
        _activeWeapon = otherWeapon;
        _isSwitching = false;
    }

    public void TakeDamage(int damage) {
        Health = Mathf.Clamp(Health - damage, 0, 100);
        HUDManager.Instance.UpdateHealth(Health);
        damageEffectController.TakeDamage();
        AudioSystem.Instance.PlaySound(hurtAudioCollection, transform.position);
        
        if (Health <= 0 && !isDead) {
            isDead = true;
            Die();
        }
    }

    public void Respawn(Vector3 position) {
        _rootTransform.position = position;
        Health = 100;
        _activeWeapon.InstantReload();
        HUDManager.Instance.UpdateHealth(Health);
		isDead = false;
    }

    private void Die() {
		_network_manager.tx_die(); //ti bi lahko dodali die msg, ki zaigra death animacijo na ostalih clientih.
        AudioSystem.Instance.PlaySound(deathAudioCollection, transform.position);
        HUDManager.Instance.EnableDeathMenu(true);
    }
}