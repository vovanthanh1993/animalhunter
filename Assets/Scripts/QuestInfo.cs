using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class QuestInfo : MonoBehaviour
{
    [Header("Panel UI")]
    public TextMeshProUGUI levelTitle;
    public TextMeshProUGUI description;
    public TextMeshProUGUI star2TimeText;
    public TextMeshProUGUI star3TimeText;

    public Button closeBtn;

    void Start() {
        closeBtn.onClick.AddListener(OnCloseButtonClicked);
    }

    void OnCloseButtonClicked() {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnEnable() {
        UpdateQuestInfo();
    }

    /// <summary>
    /// Cập nhật thông tin quest từ QuestManager
    /// </summary>
    public void UpdateQuestInfo()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.currentQuest == null)
        {
            Debug.LogWarning("QuestInfo: QuestManager.Instance hoặc currentQuest là null");
            return;
        }

        QuestData questData = QuestManager.Instance.currentQuest;

        // Hiển thị level title từ scene name
        if (levelTitle != null)
        {
            int level = GetCurrentLevelFromScene();
            levelTitle.text = "Level " + level;
        }

        // Tạo và hiển thị description từ QuestObjective
        if (description != null)
        {
            string generatedDescription = GenerateDescriptionFromObjectives(questData.objectives);
            if (!string.IsNullOrEmpty(generatedDescription))
            {
                description.text = generatedDescription;
            }
            else if (!string.IsNullOrEmpty(questData.description))
            {
                description.text = questData.description;
            }
        }

        // Hiển thị thời gian để đạt 2 sao
        if (star2TimeText != null)
        {
            string timeFormatted = FormatTime(questData.timeFor2Stars);
            star2TimeText.text = $"Complete quest in {timeFormatted}";
        }

        // Hiển thị thời gian để đạt 3 sao
        if (star3TimeText != null)
        {
            string timeFormatted = FormatTime(questData.timeFor3Stars);
            star3TimeText.text = $"Complete quest in {timeFormatted}";
        }
    }

    /// <summary>
    /// Lấy số level từ tên scene
    /// </summary>
    private int GetCurrentLevelFromScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Level"))
        {
            string levelStr = sceneName.Substring(5);
            if (int.TryParse(levelStr, out int level))
            {
                return level;
            }
        }

        return 1;
    }

    /// <summary>
    /// Tạo description từ QuestObjective array
    /// </summary>
    private string GenerateDescriptionFromObjectives(QuestObjective[] objectives)
    {
        if (objectives == null || objectives.Length == 0)
            return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Nhóm objectives theo type
        List<QuestObjective> killAnimalObjectives = new List<QuestObjective>();
        List<QuestObjective> collectItemObjectives = new List<QuestObjective>();

        foreach (var obj in objectives)
        {
            if (obj == null) continue;

            if (obj.type == QuestTargetType.KillAnimal)
            {
                killAnimalObjectives.Add(obj);
            }
            else if (obj.type == QuestTargetType.CollectItem)
            {
                collectItemObjectives.Add(obj);
            }
        }

        // Format KillAnimal objectives: "Hunt 1 deer, 4 fox"
        if (killAnimalObjectives.Count > 0)
        {
            sb.Append(FormatKillAnimalObjectives(killAnimalObjectives));
        }

        // Format CollectItem objectives
        if (collectItemObjectives.Count > 0)
        {
            if (sb.Length > 0)
            {
                sb.Append(" , ");
            }
            sb.Append(FormatCollectItemObjectives(collectItemObjectives));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format các KillAnimal objectives thành "Hunt 1 deer, 4 fox"
    /// </summary>
    private string FormatKillAnimalObjectives(List<QuestObjective> objectives)
    {
        if (objectives == null || objectives.Count == 0)
            return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("Hunt ");

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestObjective obj = objectives[i];
            string enemyName = obj.enemyType.ToString().ToLower();

            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append($"{obj.requiredAmount} {enemyName}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format các CollectItem objectives
    /// </summary>
    private string FormatCollectItemObjectives(List<QuestObjective> objectives)
    {
        if (objectives == null || objectives.Count == 0)
            return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("Collect ");

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestObjective obj = objectives[i];

            if (i > 0)
            {
                sb.Append(" , ");
            }

            if (obj.requiredAmount == 1)
            {
                sb.Append("1 item");
            }
            else
            {
                sb.Append($"{obj.requiredAmount} items");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format thời gian từ giây sang định dạng "min:ss"
    /// </summary>
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0}:{1:00}", minutes, secs);
    }
}
