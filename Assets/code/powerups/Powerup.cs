using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public PowerupEffect powerupEffect;
    public float duration = -1;

    private Vector3 rotationSpeed = new Vector3(0, 60, 0);

    private void OnTriggerEnter(Collider collision)
    {
        Destroy(gameObject);
        if (duration != -1)
        {
            PowerUpManager.Instance.HandlePowerUp(collision.gameObject,
                                                    powerupEffect, duration);
        }
        else
        {
            powerupEffect.Apply(collision.gameObject);
        }
    }

    public void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
