using UnityEngine;
using TMPro;

public class GUIPanel : MonoBehaviour
{
    public static GUIPanel Instance { get; private set; }
    public TextMeshProUGUI timeText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetTime(string time) {
        if (timeText != null)
        {
            timeText.text = time;
        }
    }
}
