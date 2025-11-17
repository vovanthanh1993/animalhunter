using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HomePanel : MonoBehaviour
{
    public Button settingButton;

    void Start()
    {
        settingButton.onClick.AddListener(OpenSetting); 
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
