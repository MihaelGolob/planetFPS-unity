using UnityEngine;

public interface IDamageable {
    public float Health { get; }
    void TakeDamage(float damage);
}
