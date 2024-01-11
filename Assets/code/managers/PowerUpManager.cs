using System;
using System.Collections;
using UnityEngine;

public class PowerUpManager : ManagerBase
{
    public static PowerUpManager Instance { get; private set; }
    public float powerupExistenceDuration = 10f;
    public GameObject[] powerupPrefabs;
    public float powerupSpawnRate = 5f;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartSpawning()
    {
        InvokeRepeating("SpawnPowerup", 5f, powerupSpawnRate);
    }

    public void SpawnPowerup()
    {
        int randomIndex = UnityEngine.Random.Range(0, powerupPrefabs.Length);

        var planets = GravityManager.Instance.GetGravitySources();
        var planetIndex = UnityEngine.Random.Range(0, planets.Count);
        
        var planetPosition = planets[planetIndex].transform.position;
        var planetRadius = planets[planetIndex].GetComponent<SphereCollider>().radius;
        
        var randomPosition = UnityEngine.Random.onUnitSphere * (planetRadius + 1);
        randomPosition += planetPosition;

        var dirFromPlanet = (planetPosition - randomPosition).normalized;
        var rotation = Quaternion.FromToRotation(Vector3.up, -dirFromPlanet) * Quaternion.LookRotation(Vector3.forward);
        
        Debug.Log($"Spawning a powerup {randomIndex} at {randomPosition}");

        GameObject powerup = Instantiate(powerupPrefabs[randomIndex], randomPosition, rotation);
    }
    
    public void HandlePowerUp(GameObject target, PowerupEffect effect, float duration)
    {
        StartCoroutine(ApplyPowerup(target, effect, duration));
    }

    private IEnumerator ApplyPowerup(GameObject target, PowerupEffect effect, float duration)
    {
        effect.Apply(target);
        yield return new WaitForSeconds(duration);
        effect.Revert(target);
    }

}