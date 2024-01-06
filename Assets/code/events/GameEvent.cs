using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Event", fileName = "New Game Event")]
public class GameEvent : ScriptableObject {
    HashSet<Action> _listeners = new ();
    
    public void Raise() {
        foreach (var listener in _listeners) {
            listener.Invoke();
        }
    }
    
    public void RegisterListener(Action listener) {
        _listeners.Add(listener);
    }
    
    public void UnregisterListener(Action listener) {
        _listeners.Remove(listener);
    }
}
