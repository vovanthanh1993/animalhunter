using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StartPanel : MonoBehaviour
{
    [Header("Panel UI")]
    public TextMeshProUGUI levelTitle;
    public List<Quest> quests;

    private PlayerLevelData currentLevelData;

    public void ShowForLevel(PlayerLevelData levelData, PlayerData playerData)
    {
        currentLevelData = levelData;

        gameObject.SetActive(true);
        levelTitle.text = "Level " + levelData.level;

        UpdateQuests(playerData?.quests);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateQuests(List<QuestStatus> questStatuses)
    {
        if (quests == null) return;
        for (int i = 0; i < quests.Count; i++)
        {
            Quest quest = quests[i];
            bool completed = questStatuses != null && i < questStatuses.Count && questStatuses[i].completed;
            quest.Init(quest.questText.text, completed);
        }
    }

    public void OnStartButtonClicked()
    {
        GameCommonUtils.LoadScene("Level" + currentLevelData.level);
        UIManager.Instance.ShowGamePlayPanel(true);
        gameObject.SetActive(false);
        UIManager.Instance.ShowHomePanel(false);
    }
}
