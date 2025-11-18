using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SettingPanel : MonoBehaviour
{
    public Button homeBtn;
    public Button closeBtn;

    private void OnEnable() {
        if(SceneManager.GetActiveScene().name == "HomeScene") 
            homeBtn.gameObject.SetActive(false);
        else homeBtn.gameObject.SetActive(true);
    }

    void Start() {
        homeBtn.onClick.AddListener(OnHomeButtonClicked);   
        closeBtn.onClick.AddListener(OnCloseButtonClicked);
    }

    public void OnHomeButtonClicked(){
        GameCommonUtils.LoadScene("HomeScene");
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        UIManager.Instance.ShowHomePanel(true);
    }

    public void OnCloseButtonClicked(){
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
