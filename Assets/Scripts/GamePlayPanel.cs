using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GamePlayPanel : MonoBehaviour
{
    public TextMeshProUGUI countDownText;
    public Image countDownImage;

    public void SetCountDown(float remainingTime, float maxTime)
    {
        if (countDownText != null)
        {
            int displayTime = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
            bool showText = displayTime > 0;
            countDownText.gameObject.SetActive(showText);
            if (showText)
            {
                countDownText.text = displayTime.ToString();
            }
        }

        if (countDownImage != null)
        {
            float normalized = (maxTime > 0f) ? Mathf.Clamp01(remainingTime / maxTime) : 0f;
            countDownImage.fillAmount = normalized;
        }
    }

    private void OnEnable()
    {
        if (countDownText != null)
        {
            countDownText.gameObject.SetActive(false);
        }

        if (countDownImage != null)
        {
            countDownImage.fillAmount = 0f;
        }
    }
}
