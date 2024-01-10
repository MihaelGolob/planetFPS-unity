using System.Collections;
using System.Collections.Generic;
using Ineor.Utils.AudioSystem;
using TMPro;
using UnityEngine;

public class HUDManager : ManagerBase {
    // singleton pattern
    public static HUDManager Instance { get; private set; }
    
    protected override void Awake() {
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
    [Header("Pause menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject soundOn;
    [SerializeField] private GameObject soundOff;
    [Header("Death menu")]
    [SerializeField] private GameObject deathMenu;
    
    // private
    NetworkManager _networkManager;
    public bool IsPauseMenuEnabled => pauseMenu.activeSelf || deathMenu.activeSelf;

    private void Start() {
        var volume = AudioSystem.Instance.GetGroupVolume("Master");
        ToggleSound(volume > 0f);
        _networkManager = NetworkManager.game_object.GetComponent<NetworkManager>();
    }
		
	private void OnEnable()
	{
		NetworkManager.disconnectedCallback = ExitToMainMenu;
	}

	private void OnDisable()
	{
		NetworkManager.disconnectedCallback = null;
	}


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
    
    public void ExitToMainMenu() {
        _networkManager.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnRespawnButton() {
        GameManager.Instance.RespawnPlayer();
        EnableDeathMenu(false);
    }
    
    public void EnableDeathMenu(bool enable) {
        deathMenu.SetActive(enable);
        if (enable) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public void ToggleSound(bool isOn) {
        soundOn.SetActive(isOn);
        soundOff.SetActive(!isOn);

        AudioSystem.Instance.SetGroupVolume("Master", isOn ? 1f : 0f);
    }
}
