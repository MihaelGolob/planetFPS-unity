using System;
using System.Collections;
using System.Collections.Generic;
using Ineor.Utils.AudioSystem;
using Unity.VisualScripting;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public PowerupEffect powerupEffect;
    public float duration = -1;
    public float selfDestructTime;
    
    [Header("Audio Collections")]
    [SerializeField] private AudioCollection pickupAudioCollection;

    private Vector3 rotationSpeed = new Vector3(0, 60, 0);

    private void Start()
    {
        StartCoroutine(SelfDestructCoroutine());
    }
    
    private IEnumerator SelfDestructCoroutine()
    {
        yield return new WaitForSeconds(selfDestructTime);
        Destroy(gameObject); 
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioSystem.Instance.PlaySound(pickupAudioCollection, transform.position);
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
        
        Debug.Log(collision.tag);
        Destroy(gameObject);
    }

    public void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
