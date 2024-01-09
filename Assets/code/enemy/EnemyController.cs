using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    [SerializeField] [Range(0.01f, 1f)] private float animationUpdateFrequency = 0.3f;
    
    // private variables
    private Animator _animator;

    private Vector3 _oldPosition;
    private float _lastAnimationUpdateTime;
    
    // animator hashed parameters
    private readonly int _moveSpeedParameter = Animator.StringToHash("MoveSpeed");
    private readonly int _jumpParameter = Animator.StringToHash("Jump");
    
    private void Start() {
        _animator = GetComponent<Animator>();
    }

    private void Update() {
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
}
