using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject homePanel;

    public GameObject selectLevelPanel;

    public StartPanel startPanel;

    public GamePlayPanel gamePlayPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoadingPanel(bool isShow) {

    }

    public void ShowSelectLevelPanel(bool isShow) {
        if (selectLevelPanel != null)
        {
            selectLevelPanel.SetActive(isShow);
        }
    }

    public void ShowGamePlayPanel(bool isShow) {
        if (gamePlayPanel != null)
        {
            gamePlayPanel.gameObject.SetActive(isShow);
        }
    }
}
