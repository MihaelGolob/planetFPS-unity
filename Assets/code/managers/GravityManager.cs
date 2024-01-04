using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityManager : ManagerBase{
    // singleton pattern
    public static GravityManager Instance { get; private set; }
    
    protected override void Awake() {
        base.Awake();
        
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }
    
    private List<GravitySource> _gravitySources = new();
    
    public void RegisterGravitySource(GravitySource gravitySource) {
        _gravitySources.Add(gravitySource);
    }
    
    public void UnregisterGravitySource(GravitySource gravitySource) {
        _gravitySources.Remove(gravitySource);
    }

    public Vector3 GetGravity(Vector3 position) {
        var gravity = Vector3.zero;
        var closestDistance = float.MaxValue;
        
        // find the nearest gravity source
        foreach (var source in _gravitySources) {
            var distance = Vector3.Distance(source.transform.position, position);
            if (distance < closestDistance) {
                closestDistance = distance;
                gravity = source.transform.position;
            }
        }

        return gravity;
    } 
}