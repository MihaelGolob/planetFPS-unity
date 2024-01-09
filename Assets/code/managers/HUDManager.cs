using System.Collections;
using System.Collections.Generic;
using Ineor.Utils.AudioSystem;
using TMPro;
using UnityEngine;

public class HUDManager : ManagerBase {
    // singleton pattern
    public static HUDManager Instance { get; private set; }
    
    protected override void Awake() {
        base.Awake();
        
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }
    
    public bool IsPauseMenuEnabled => pauseMenu.activeSelf;

    private void Start() {
        var volume = AudioSystem.Instance.GetGroupVolume("Master");
        ToggleSound(volume > 0f);
    }
    
    // inspector assigned
    [Header("UI elements")] 
    [SerializeField] private TMP_Text ammoCountText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text killCountText;
    [Header("Pause menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject soundOn;
    [SerializeField] private GameObject soundOff;

    public void UpdateAmmoCount(int ammo) {
        ammoCountText.text = ammo.ToString();
    }
    
    public void UpdateHealth(int health) {
        healthText.text = health.ToString();
    }
    
    public void UpdateKillCount(int killCount) {
        killCountText.text = killCount.ToString();
    }

    public void EnablePauseMenu(bool enable) {
        pauseMenu.SetActive(enable);
    }
    
    public void ToggleSound(bool isOn) {
        soundOn.SetActive(isOn);
        soundOff.SetActive(!isOn);

        AudioSystem.Instance.SetGroupVolume("Master", isOn ? 1f : 0f);
    }
}
