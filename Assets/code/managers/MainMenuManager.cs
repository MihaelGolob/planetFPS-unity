using System.Text;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text serverIpText;
    [SerializeField] private string gameSceneName;

    NetworkManager networkManager;

    private void OnEnable()
    {
        networkManager = NetworkManager.game_object.GetComponent<NetworkManager>();
        NetworkManager.connected_callback = OnConnected;
        NetworkManager.disconnectedCallback = OnDisconnected;
    }
    private void OnDisable()
    {
        NetworkManager.connected_callback = null;
        NetworkManager.disconnectedCallback = null;
    }

    public void OnPlayButtonPressed()
    {
        networkManager.Connect(serverIpText.GetParsedText().Replace("\u200B", ""));
        //TODO  goto Loading screen
    }
    public void OnConnected()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    public void OnDisconnected()
    {
        //i guess da to to ne bo nikoli poklicalo, ker se tale gameobject zbrise ko nalozimo game sceno.

        //Nazaj v main menu, igralce ze trenutno pobrise network manager
    }

}
