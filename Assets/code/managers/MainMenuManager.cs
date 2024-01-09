using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_InputField IpInputField;
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

    private void Start() {
        // use web request to fetch server IP
        StartCoroutine(GetRequest("https://benjaminlipnik.eu/public/pages/planet_runner/?servers"));
    }

    private IEnumerator GetRequest(string url) {
        using var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success) {
            var serverIp = webRequest.downloadHandler.text;
            // remove new lines
            serverIp = Regex.Replace(serverIp, @"\t|\n|\r", "");
            IpInputField.text = serverIp;
        } else {
            Debug.Log(webRequest.error);
        }
    }

    public void OnPlayButtonPressed()
    {
        networkManager.Connect(IpInputField.textComponent.GetParsedText().Replace("\u200B", ""));
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
