using System.Collections;
using System.Collections.Generic;
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
    
    // inspector assigned
    [Header("UI elements")] 
    [SerializeField] private TMP_Text ammoCountText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text killCountText;

    public void UpdateAmmoCount(int ammo) {
        ammoCountText.text = ammo.ToString();
    }
    
    public void UpdateHealth(int health) {
        healthText.text = health.ToString();
    }
    
    public void UpdateKillCount(int killCount) {
        killCountText.text = killCount.ToString();
    }
}
