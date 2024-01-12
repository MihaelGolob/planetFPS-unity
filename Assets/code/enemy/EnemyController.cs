using TMPro;
//using UnityEditor.Animations;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    [SerializeField] private Canvas nameCanvas;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] [Range(0.01f, 1f)] private float animationUpdateFrequency = 0.3f;
    
    // private variables
    private Animator _animator;

    private Vector3 _oldPosition;
    private float _lastAnimationUpdateTime;
    private string _name;
    private Camera _camera;
    
    // animator hashed parameters
    private readonly int _moveSpeedParameter = Animator.StringToHash("MoveSpeed");
    private readonly int _jumpParameter = Animator.StringToHash("Jump");
    
    private void Start() {
        _animator = GetComponent<Animator>();
    }

    private void Update() {
        nameText.transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
        var textRotation = nameText.transform.localEulerAngles;
        textRotation.x = 0;
        textRotation.z = 0;
        nameText.transform.localEulerAngles = textRotation;
        
        if (Time.time - _lastAnimationUpdateTime < animationUpdateFrequency) {
            return;
        }
        _lastAnimationUpdateTime = Time.time;
        
        var position = transform.position;
        var moveDirection = position - _oldPosition;
        if (moveDirection.magnitude < 0.001f) {
            _animator.SetFloat(_moveSpeedParameter, 0);
            return;
        }
        
        // check if we are moving forward or backwards
        var forward = transform.forward;
        var dot = Vector3.Dot(moveDirection.normalized, forward);

        if (dot < 0) {
            _animator.SetFloat(_moveSpeedParameter, -1);
        } else {
            _animator.SetFloat(_moveSpeedParameter, 1);
        }
        
        _oldPosition = position;
    }

    public void SetName(string playerName) {
        _name = playerName;
        Debug.Log($"Setting name to {_name}");
        if (_camera == null)
            _camera = Camera.main;
        
        nameText.text = _name;
        nameCanvas.worldCamera = _camera;
    }
}
