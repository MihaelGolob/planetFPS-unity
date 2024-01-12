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
	System.String serverIp = "127.0.0.1";

    private void OnEnable()
    {
        networkManager = NetworkManager.game_object.GetComponent<NetworkManager>();
        NetworkManager.connected_callback = OnConnected;
    }
    
    private void OnDisable()
    {
        NetworkManager.connected_callback = null;
    }

    private void Start() { //To bi se moglo zagnati za vsako, ko gremo v main menu, oz. ip bi se mogu za vsako nastavit.

		// use web request to fetch server IP
        StartCoroutine(GetRequest("https://benjaminlipnik.eu/public/pages/planet_runner/?servers"));
    }

    private IEnumerator GetRequest(string url) {
        using var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success) {
            serverIp = webRequest.downloadHandler.text;
            // remove new lines
            serverIp = Regex.Replace(serverIp, @"\t|\n|\r", "");
            IpInputField.text = serverIp;
        } else {
            Debug.Log(webRequest.error);
        }
    }

    public void OnPlayButtonPressed()
    {
        networkManager.Connect(IpInputField.textComponent.GetParsedText().Replace("\u200B", ""), usernameText.GetParsedText());
    }
    public void OnConnected()
    {
        SceneManager.LoadScene(gameSceneName);
        PowerUpManager.Instance.StartSpawning();
    }

}
