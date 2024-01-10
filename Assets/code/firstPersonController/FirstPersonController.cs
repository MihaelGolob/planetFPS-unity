using System.Collections;
using UnityEngine;
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
    [SerializeField] private float moveSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private Vector2 upDownAngleRotation = new (-85f, 75f);
    [SerializeField] private float jumpHeight = 5;
    
    [Header("Shooting")]
    [SerializeField] private WeaponBase weapon;
    [SerializeField] private FullScreenDamageController damageEffectController;

    [Header("Gravity")] 
    [SerializeField] private IsGroundedComponent isGroundedComponent;
    [SerializeField] private float gravityAcceleration = 9.8f;
    
    private Vector3 _gravitySource;
    
    // public members
    public int Health { get; private set; } = 100;
    
    // private variables
    private Transform _rootTransform;
    private Transform _bodyTransform;
    private Transform _cameraTransform;
    private Rigidbody _rigidbody;
    private NetworkManager _network_manager;

    private bool _isGrounded;
    private float _gravitySpeed;
    private float _lastJumpTime;

    private bool _canZipline;
    private Collider _ziplineEnterCollider;
    private Zipline _activeZipline;
    private bool _isDead;
    public bool _isZiplining { get; private set; } = false;
    
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
        _network_manager.tx_spawn_player(_rootTransform.position);
        
        HUDManager.Instance.UpdateHealth(Health);
    }
    
    void CalculateVelocity()
    {
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
        if (_isZiplining) return;

        Vector3 moveDir =
            _bodyTransform.forward * Input.GetAxis("Vertical") +
            _bodyTransform.right * Input.GetAxis("Horizontal");

        if (moveDir.magnitude != 0)
            moveDir = moveDir.normalized;

        _isGrounded = isGroundedComponent.isGrounded;

        var speed = _isGrounded ? moveSpeed : airSpeed;
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
        if (_isGrounded && _gravitySpeed <= 0)
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
        if (Input.GetMouseButton(0)) {
            //ne, ker tole ne uposteva ziplina
            //weapon.Shoot(_rigidbody.velocity, _cameraTransform.forward, transform.position);
            weapon.Shoot(player_velocity, _cameraTransform.forward, transform.position);

        }
    }
    
    private void Reload() {
        if (Input.GetKeyDown(KeyCode.R)) {
            weapon.Reload();
        }
    }

    private void Zipline() {
        if (!_canZipline || _isZiplining) return;
        
        if (Input.GetKeyDown(KeyCode.E)) {
            _isZiplining = true;
            var info = _activeZipline.GetZiplineInfo(_ziplineEnterCollider);
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

    public void TakeDamage(int damage) {
        Health = Mathf.Clamp(Health - damage, 0, 100);
        HUDManager.Instance.UpdateHealth(Health);
        damageEffectController.TakeDamage();
        
        
        if (Health <= 0 && !_isDead) {
            _isDead = true;
            Die();
        }
    }

    public void Respawn(Vector3 position) {
        _rootTransform.position = position;
        Health = 100;
        HUDManager.Instance.UpdateHealth(Health);
		_isDead = false;
    }

    private void Die() {
        HUDManager.Instance.EnableDeathMenu(true);
    }
}