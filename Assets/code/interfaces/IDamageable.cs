using UnityEngine;

public interface IDamageable {
    public int Health { get; }
    void TakeDamage(int damage);
}
