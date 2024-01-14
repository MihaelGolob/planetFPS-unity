using System;
using System.Collections;
using System.Collections.Generic;
using Ineor.Utils.AudioSystem;
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
    [SerializeField] private AudioCollection backgroundMusic;
    
    // private
    private NetworkManager _networkManager;
    private FirstPersonController _player;
    
    private Queue<(string, Vector3, Action<GameObject>)> _enemySpawnQueue = new ();
    
    private void Start() {
        _networkManager = NetworkManager.game_object.GetComponent<NetworkManager>();
        AudioSystem.Instance.PlaySound(backgroundMusic, Vector3.zero);
    }

    private void Update() {
        if (_enemySpawnQueue.Count > 0) {
            var (name, position, onSpawned) = _enemySpawnQueue.Dequeue();
            SpawnEnemy(name, position, onSpawned);
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

    public void QueueSpawnEnemy(String name, Vector3 position, Action<GameObject> onSpawned) {
        // check if you are in game scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game") {
            _enemySpawnQueue.Enqueue((name, position, onSpawned));
            return;
        }
        
        SpawnEnemy(name, position, onSpawned);
    }
    
    private void SpawnEnemy(string name, Vector3 position, Action<GameObject> onSpawned) {
        var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        var enemyComponent = enemy.GetComponent<EnemyController>();
        enemyComponent.SetName(name);
        onSpawned?.Invoke(enemy);
    }
    
    private Vector3 GetRandomSpawnPosition() {
        // get random planet
        var planets = GravityManager.Instance.GetGravitySources();
        var planetIndex = Random.Range(0, planets.Count);
        
        // get random position on planet
        var planetPosition = planets[planetIndex].transform.position;
        var planetRadius = planets[planetIndex].GetComponent<SphereCollider>().radius * planets[planetIndex].transform.localScale.x;
        
        var randomPosition = Random.onUnitSphere * (planetRadius + 3);
        randomPosition += planetPosition;
        
        return randomPosition;
    }
}
