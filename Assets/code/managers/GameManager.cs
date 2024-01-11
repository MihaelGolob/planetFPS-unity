using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : ManagerBase {
    // singleton pattern
    public static GameManager Instance { get; private set; }
    
    protected override void Awake() {
        base.Awake();
        
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }
    
    // inspector assigned
    [SerializeField] private GameObject enemyPrefab;
    
    // private
    private NetworkManager _networkManager;
    private FirstPersonController _player;
    
    private Queue<(Vector3, Action<GameObject>)> _enemySpawnQueue = new ();
    
    private void Start() {
        _networkManager = NetworkManager.game_object.GetComponent<NetworkManager>();
    }

    private void Update() {
        if (_enemySpawnQueue.Count > 0) {
            var (position, onSpawned) = _enemySpawnQueue.Dequeue();
            SpawnEnemy(position, onSpawned);
        }
    }

    public void RespawnPlayer() {
        if (_player == null) {
            _player = FindObjectOfType<FirstPersonController>();
        }
        
        var position = GetRandomSpawnPosition();
        _player.Respawn(position);
        _networkManager.tx_spawn_player(position);
    }

    public void QueueSpawnEnemy(Vector3 position, Action<GameObject> onSpawned) {
        // check if you are in game scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game") {
            _enemySpawnQueue.Enqueue((position, onSpawned));
            return;
        }
        
        SpawnEnemy(position, onSpawned);
    }
    
    private void SpawnEnemy(Vector3 position, Action<GameObject> onSpawned) {
        var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        onSpawned?.Invoke(enemy);
    }
    
    private Vector3 GetRandomSpawnPosition() {
        // get random planet
        var planets = GravityManager.Instance.GetGravitySources();
        var planetIndex = Random.Range(0, planets.Count);
        
        // get random position on planet
        var planetPosition = planets[planetIndex].transform.position;
        var planetRadius = planets[planetIndex].GetComponent<SphereCollider>().radius;
        
        var randomPosition = Random.onUnitSphere * (planetRadius + 3);
        randomPosition += planetPosition;
        
        return randomPosition;
    }
}
