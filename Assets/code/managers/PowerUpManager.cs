using System;
using System.Collections;
using UnityEngine;

public class PowerUpManager : ManagerBase
{
    public static PowerUpManager Instance { get; private set; }
    public float powerupExistenceDuration = 10f;
    public GameObject[] powerupPrefabs;
    public float powerupSpawnRate = 5f;

    private NetworkManager _networkManager;

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
        _networkManager = NetworkManager.game_object.GetComponent<NetworkManager>();
    }
    
    public void SpawnPowerup()
    {
        int randomIndex = UnityEngine.Random.Range(0, powerupPrefabs.Length);

        var planets = GravityManager.Instance.GetGravitySources();
        var planetIndex = UnityEngine.Random.Range(0, planets.Count);

        if (planets.Count == 0)
            return;
        
        var planetPosition = planets[planetIndex].transform.position;
        var planetRadius = planets[planetIndex].GetComponent<SphereCollider>().radius;
        
        var randomPosition = UnityEngine.Random.onUnitSphere * (planetRadius + 1);
        randomPosition += planetPosition;

        var dirFromPlanet = (planetPosition - randomPosition).normalized;
        var rotation = Quaternion.FromToRotation(Vector3.up, -dirFromPlanet) * Quaternion.LookRotation(Vector3.forward);
        
        var powerup = Instantiate(powerupPrefabs[randomIndex], randomPosition, rotation);
        _networkManager.tx_spawn_powerup(randomPosition, rotation, randomIndex);
    }

    public void SpawnPowerup(Vector3 pos, Quaternion rot, int index)
    {
        Instantiate(powerupPrefabs[index], pos, rot);
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