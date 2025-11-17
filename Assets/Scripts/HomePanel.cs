using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HomePanel : MonoBehaviour
{
    public Button startButton;
    public Button settingButton;

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        settingButton.onClick.AddListener(OpenSetting); 
    }

    void StartGame()
    {
        GameCommonUtils.LoadScene("GameLevel");
        gameObject.SetActive(false);
    }

    void OpenSetting()
    {
        GameCommonUtils.LoadScene("Setting");
    }

    void ExitGame()
    {
        Application.Quit();
    }
}
