using System.Collections.Generic;
using UnityEngine;

namespace Ineor.Utils.AudioSystem {
    
public enum PlayOrder {InOrder, Random, Reverse}

/// <summary>
/// A scriptable object that holds all info about the sounds played
/// </summary>
[CreateAssetMenu(fileName = "New Audio Collection")]
public class AudioCollection : ScriptableObject {
    // inspector assigned
    [SerializeField] private string _audioGroup;
    [SerializeField] [Range(0f, 1f)] private float _volume = 1f;
    [SerializeField] [Range(0f, 1f)] private float _spatialBlend = 1f;
    [SerializeField] [Range(0, 256)] private byte _priority = 128;
    [SerializeField] private PlayOrder _playOrder = PlayOrder.Random;
    [SerializeField] private bool _loop = false;
    [SerializeField] private bool _stopOnSceneChange = true;
    [SerializeField] private List<AudioClip> _audioClips = new();
    
    // private
    private int _currentClipIndex = 0;
    
    // Getters
    public string AudioGroup => _audioGroup;
    public float Volume => _volume;
    public float SpatialBlend => _spatialBlend;
    public byte Priority => _priority;
    public bool Loop => _loop;
    public bool StopOnSceneChange => _stopOnSceneChange;

    /// <summary>
    /// Returns specified audio clip in the audio collection
    /// </summary>
    /// <param name="i"></param>
    public AudioClip this[int i] {
        get {
            if (_audioClips.Count == 0) {
                Debug.LogError("No audio clips in collection");
                return null;
            }
            if (i < 0 || i >= _audioClips.Count) {
                Debug.LogError("Index out of range");
                return null;
            }
            
            return _audioClips[i];
        }
    }
    
    /// <summary>
    /// Returns a clip from the audio collection based on the play order
    /// </summary>
    /// <returns></returns>
    public AudioClip AudioClip {
        get {
            if (_audioClips.Count == 0) {
                Debug.LogError("No audio clips in collection");
                return null;
            }

            return _playOrder switch {
                PlayOrder.InOrder => _audioClips[++_currentClipIndex % _audioClips.Count],
                PlayOrder.Random => _audioClips[Random.Range(0, _audioClips.Count)],
                PlayOrder.Reverse => _audioClips[--_currentClipIndex % _audioClips.Count],
                _ => null
            };
        }
    }
}
}
