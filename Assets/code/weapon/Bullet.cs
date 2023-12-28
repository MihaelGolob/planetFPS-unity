using UnityEngine;

public class Bullet : MonoBehaviour {
    // parameters
    private Vector3 _direction;
    private float _speed;
    private float _lifetime;
    private float _damage;
    
    // private variables
    private bool _isInitialized;

    public void Init(Vector3 direction, float speed, float damage, float lifetime) {
        _direction = direction;
        _speed = speed;
        _lifetime = lifetime;
        _damage = damage;
        
        _isInitialized = true;
    }
    
    private void Update() {
        if (!_isInitialized) return;
        
        transform.position += _direction * (_speed * Time.deltaTime);
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0) {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if (!_isInitialized) return;
        
        var damageable = other.GetComponent<IDamageable>();
        damageable?.TakeDamage(_damage);
        
        Destroy(gameObject);
    }
}
