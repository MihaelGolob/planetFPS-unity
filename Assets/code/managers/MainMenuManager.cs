using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text serverIpText;
    [SerializeField] private string gameSceneName;

    public void PlayGame() {
        SceneManager.LoadScene(gameSceneName);
    }
}
