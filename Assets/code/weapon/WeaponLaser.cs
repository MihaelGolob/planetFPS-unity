using UnityEngine;

public class WeaponLaser : WeaponBase {
    public override Bullet CreateBullet(Vector3 pos, Vector3 velocity) {
        var bulletObject = Instantiate(bulletPrefab, pos, Quaternion.identity);
        var bullet = bulletObject.GetComponent<Bullet>();
        bullet.Init(velocity, bulletDamage, bulletLifetime, false);

        return bullet;
    }
}
