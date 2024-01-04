using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Ineor.Utils.AudioSystem {

public class PoolItem {
    public GameObject GameObject;
    public AudioSource AudioSource;
    public Transform Transform;
    public byte Priority = 128;
    public bool IsPlaying;
    public bool Loop;
    public ulong Id;
    public bool StopOnSceneChange;

    private bool paused;

    public void Pause() {
        AudioSource.Pause();
        IsPlaying = false;
        paused = true;
    }

    public void UnPause() {
        if (paused) AudioSource.UnPause();
        IsPlaying = true;
    }
}

/// <summary>
/// The singleton class which provides the interface for using the audio system.
/// It uses the object pool programming pattern to manage the audio sources.
///
/// Created by: Mihael Golob on 05. 09. 2022
/// </summary>
public class AudioSystem : MonoBehaviour {
    // serialized
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private int _maxSounds = 15;

    // singleton pattern
    public static AudioSystem Instance { get; private set; }

    // internal variables
    private List<PoolItem> _pool = new ();
    private Dictionary<string, AudioMixerGroup> _mixerGroups = new (); // cache for mixer groups

    // ---------------------------------------------------------- U N I T Y -------------------------------------------------------------------------------

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake() {
        // don't destroy when changing scenes
        DontDestroyOnLoad(gameObject);
        // only one object of this type can exist
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }

        CacheMixerGroups();
        GeneratePool();
    }
    
    // ---------------------------------------------------------- P R I V A T E ---------------------------------------------------------------------------
    
    // SETUP
    /// <summary>
    /// Generates a pool of game objects with audio sources.
    /// </summary>
    private void GeneratePool() {
        for (var i = 0; i < _maxSounds; i++) {
            // instantiate the game object
            var go = new GameObject("Pool item");
            var audioSource = go.AddComponent<AudioSource>();
            go.transform.parent = gameObject.transform;
            
            // configure pool item
            var poolItem = new PoolItem {
                GameObject = go,
                AudioSource = audioSource,
                Transform = go.transform,
                Priority = 128,
                IsPlaying = false,
                Id = (ulong) i
            };
            // disable game object
            poolItem.GameObject.SetActive(false);
            // add to the pool
            _pool.Add(poolItem);
        }
    }
    
    /// <summary>
    /// We need to cache mixer groups so we can only pass the group name to the Play method.
    /// </summary>
    private void CacheMixerGroups() {
        var groups = _audioMixer.FindMatchingGroups(string.Empty);
        foreach (var group in groups) {
            _mixerGroups.Add(group.name, group);
        }
    }

    /// <summary>
    /// stop certain sounds when the scene changes
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        foreach (var item in _pool) {
            if (item.IsPlaying && item.StopOnSceneChange) {
                StopSoundInternal(item);
            }
        }
    }
    
    // SOUND PLAYING METHODS ---------------------------------------------------
    private ulong ConfigurePoolObject(PoolItem poolItem, string group, AudioClip clip, Vector3 position, float volume, float spatialBlend, 
        byte priority, float clipStartTime, bool loop, bool stopOnSceneChange) {
        // configure the game object
        poolItem.GameObject.SetActive(true);
        poolItem.GameObject.transform.position = position;
        // configure the audio source
        poolItem.AudioSource.clip = clip;
        poolItem.AudioSource.outputAudioMixerGroup = _mixerGroups[group];
        poolItem.AudioSource.volume = volume;
        poolItem.AudioSource.spatialBlend = spatialBlend;
        poolItem.AudioSource.time = clipStartTime;
        // configure pool item
        poolItem.Transform.position = position;
        poolItem.Priority = priority;
        poolItem.IsPlaying = true;
        poolItem.Loop = loop;
        poolItem.StopOnSceneChange = stopOnSceneChange;
        // play the audio clip
        poolItem.AudioSource.Play();
        // stop sound when it finishes playing
        StartCoroutine(RestartSound(clip.length, poolItem));
        // return the id
        return poolItem.Id;
    }

    /// <summary>
    /// Play a sound from the pool.
    /// </summary>
    /// <param name="group"></param>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    /// <param name="spatialBlend"></param>
    /// <param name="priority"></param>
    /// <param name="clipStartTime"></param>
    /// <param name="loop"></param>
    /// <param name="stopOnSceneChange"></param>
    /// <returns></returns>
    private ulong PlaySound(string group, AudioClip clip, Vector3 position, float volume, float spatialBlend, 
        byte priority = 128, float clipStartTime = 0f, bool loop = false, bool stopOnSceneChange = true) {
        // exit clauses 
        if (!_mixerGroups.ContainsKey(group)) {
            Debug.LogError($"AudioSystem: Mixer group {group} does not exist.");
            return 0;
        }
        if (clip == null || volume.Equals(0f)) return 0;
        
        // find the first available pool item
        var poolItem = _pool.Find(item => !item.IsPlaying);
        if (poolItem == null) {
            // find an item with less importance if there are no free audio sources
            poolItem = _pool.Find(item => item.Priority < priority);
            if (poolItem == null) {
                // no free audio sources and no items with less importance
                Debug.LogWarning($"AudioSystem: No free audio sources and no items with less importance. Tried to play: {clip.name}");
                return 0;
            }
        }
        
        return ConfigurePoolObject(poolItem, group, clip, position, volume, spatialBlend, priority, clipStartTime, loop, stopOnSceneChange);
    }
    
    // internal coroutine so the user does not have to know the implementation details
    private IEnumerator PlaySoundDelayedInternal(float delay, string group, AudioClip clip, Vector3 position, float volume, float spatialBlend,
        byte priority = 128, float clipStartTime = 0f, bool loop = false, bool stopOnSceneChange = true) {
        
        yield return new WaitForSeconds(delay);
        PlaySound(group, clip, position, volume, spatialBlend, priority, clipStartTime, loop, stopOnSceneChange);
    }

    private IEnumerator FadeInSoundInternal(ulong id, float fadeInTime) {
        var poolItem = _pool.Find(item => item.Id == id);
        // mute the audio source
        var volume = poolItem.AudioSource.volume;
        poolItem.AudioSource.volume = 0f;
        // start fading in
        var elapsedTime = 0f;
        while (elapsedTime < fadeInTime) {
            elapsedTime += Time.deltaTime;
            poolItem.AudioSource.volume = Mathf.Lerp(0f, volume, elapsedTime / fadeInTime);
            yield return null;
        }
    }
    
    // STOP STOPPING METHODS ----------------------------------------------------
    private IEnumerator StopSoundDelayedInternal(float delay, ulong id) {
        yield return new WaitForSeconds(delay);
        StopSound(id);
    }
    
    private void StopSoundInternal(PoolItem poolItem) {
        // reset audio source
        poolItem.AudioSource.Stop();
        poolItem.AudioSource.clip = null;
        poolItem.IsPlaying = false;
        poolItem.Loop = false;
        // disable game object
        poolItem.GameObject.SetActive(false);
    }

    private IEnumerator RestartSound(float delay, PoolItem poolItem) {
        yield return new WaitForSeconds(delay);
        if (poolItem.Loop) {
            // play from the beginning
            poolItem.AudioSource.Stop();
            poolItem.AudioSource.time = 0f;
            poolItem.AudioSource.Play();
            // restart the coroutine
            StartCoroutine(RestartSound(poolItem.AudioSource.clip.length, poolItem));
        }
        else {
            StopSoundInternal(poolItem);
        }
    }

    private IEnumerator StopSoundWithFadeOutInternal(ulong id, float fadeoutTime) {
        var poolItem = _pool.Find(item => item.Id == id);
        if (poolItem == null) yield break;
        
        var initialVolume = poolItem.AudioSource.volume;
        var time = 0f;
        while (time < fadeoutTime) {
            time += Time.deltaTime;
            poolItem.AudioSource.volume = Mathf.Lerp(initialVolume, 0f, time / fadeoutTime);
            yield return null;
        }
        StopSoundInternal(poolItem);
    }
    
    // ---------------------------------------------------------- P U B L I C    A P I -----------------------------------------------------------------------
    // SOUND PLAYING METHODS ----------------------------------------------------
    
    /// <summary>
    /// Play a sound from an audio collection.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public ulong PlaySound(AudioCollection collection, AudioClip clip, Vector3 position) {
        if (collection == null) {
            Debug.LogError($"AudioSystem: Audio collection is null.");
            return 0;
        }
        
        return PlaySound(collection.AudioGroup, clip, position, collection.Volume, collection.SpatialBlend, collection.Priority, 0f, collection.Loop, collection.StopOnSceneChange);
    }

    /// <summary>
    /// Play a random sound from the audio collection
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="position"></param>
    public ulong PlaySound(AudioCollection collection, Vector3 position) => PlaySound(collection, collection.AudioClip, position);

    /// <summary>
    /// Audio collection version of PlaySoundDelayed.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="collection"></param>
    /// <param name="position"></param>
    public void PlaySoundDelayed(float delay, AudioCollection collection, Vector3 position) =>
        StartCoroutine(PlaySoundDelayedInternal(delay, collection.AudioGroup, collection.AudioClip, position, collection.Volume, collection.SpatialBlend, collection.Priority, 0f, collection.Loop, collection.StopOnSceneChange));

    /// <summary>
    /// Play and audio clip with a smooth volume transition.
    /// </summary>
    /// <param name="fadeInTime"></param>
    /// <param name="collection"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public ulong PlaySoundWithFadeIn(float fadeInTime, AudioCollection collection, Vector3 position) {
        var id = PlaySound(collection, position);
        StartCoroutine(FadeInSoundInternal(id, fadeInTime));
        return id;
    }
    
    // SOUND STOPPING METHODS ----------------------------------------------------
    /// <summary>
    /// Stop playing s sound with the specific id
    /// </summary>
    /// <param name="id"></param>
    public void StopSound(ulong id) {
        var poolItem = _pool.Find(item => item.Id == id);
        if (poolItem == null) {
            Debug.LogWarning($"AudioSystem: Could not find pool item with id {id}");
            return;
        }
        
        StopSoundInternal(poolItem);
    }
    
    public void StopSoundDelayed(float delay, ulong id) => StartCoroutine(StopSoundDelayedInternal(delay, id));

    /// <summary>
    /// Gradually lower the volume of a sound and then stop it.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fadeoutTime"></param>
    public void StopSoundWithFadeout(ulong id, float fadeoutTime) => StartCoroutine(StopSoundWithFadeOutInternal(id, fadeoutTime));
    
    // SOUND PAUSE METHODS --------------------------------------------------------
    public void PauseSound(ulong id) {
        var poolItem = _pool.Find(item => item.Id == id);
        if (poolItem == null) {
            Debug.LogWarning($"AudioSystem: Could not find pool item with id {id}");
            return;
        }
        
        poolItem.Pause();
    }
    
    public void ResumeSound(ulong id) {
        var poolItem = _pool.Find(item => item.Id == id);
        if (poolItem == null) {
            Debug.LogWarning($"AudioSystem: Could not find pool item with id {id}");
            return;
        }
        
        poolItem.UnPause();
    }

    /// <summary>
    /// Pause all sounds currently playing
    /// </summary>
    public void PauseAll() {
        foreach (var item in _pool) {
            if (item.IsPlaying) item.Pause();
        }
    }

    /// <summary>
    /// Resume all active sounds.
    /// </summary>
    public void ResumeAll() {
        foreach (var item in _pool) {
            if (!item.IsPlaying) item.UnPause();
        }
    }

    /// <summary>
    /// stop all playing sounds
    /// </summary>
    /// <returns></returns>
    public void StopAll() {
        foreach (var item in _pool) {
            if (item.IsPlaying) StopSoundInternal(item);
        }
    }
    
    // SOUND VOLUME METHODS -------------------------------------------------------
    
    /// <summary>
    /// Sets the specified group's volume.
    /// (you need expose group volume parameter in the AudioMixer)
    /// </summary>
    /// <param name="group"></param>
    /// <param name="volume"></param>
    public void SetGroupVolume(string group, float volume) {
        if (!_mixerGroups.TryGetValue(group, out var mixerGroup)) {
            Debug.LogError("AudioSystem: Invalid mixer group name");
            return;
        }
        // map volume from 0-1 to -80-0
        volume = Mathf.Lerp(-80, 0, volume);
        // set attenuation of the mixer group
        mixerGroup.audioMixer.SetFloat($"{mixerGroup.name}Volume", volume);
    }

    /// <summary>
    /// Sets the volume of a sound with the specified id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="volume"></param>
    public void SetSoundVolume(ulong id, float volume) {
        var poolItem = _pool.Find(item => item.Id == id);
        if (poolItem == null) {
            Debug.LogWarning($"AudioSystem: Could not find pool item with id {id}");
            return;
        }
        
        poolItem.AudioSource.volume = volume;
    }
}

}