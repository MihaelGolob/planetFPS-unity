using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    // private
    private NetworkManager _networkManager;
    private FirstPersonController _player;
    
    private void Start() {
        _networkManager = NetworkManager.game_object.GetComponent<NetworkManager>();
    }
    
    public void RespawnPlayer() {
        if (_player == null) {
            _player = FindObjectOfType<FirstPersonController>();
        }
        
        _networkManager.tx_destroy_player();
        var position = GetRandomSpawnPosition();
        _player.Respawn(position);
        _networkManager.tx_spawn_player(position);
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
