using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestData currentQuest;
    
    public Dictionary<string, int> progress = new Dictionary<string, int>();

    private bool questCompleted = false;
    
    private float gameStartTime;
    private float gameElapsedTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Khởi tạo progress
        questCompleted = false;
        gameStartTime = Time.time;
        gameElapsedTime = 0f;

        foreach (var obj in currentQuest.objectives)
        {
            progress[obj.targetId] = 0;
        }
    }

    void Update()
    {
        if (!questCompleted)
        {
            gameElapsedTime = Time.time - gameStartTime;
        }
        GUIPanel.Instance.SetTime(GetGameTimeFormatted());
    }

    public void OnEnemyKilled(string enemyId)
    {
        if (!progress.ContainsKey(enemyId))
            return;

        progress[enemyId]++;

        Debug.Log($"{enemyId} progress: {progress[enemyId]} / {GetRequiredAmount(enemyId)}");

        CheckQuestComplete();
    }

    int GetRequiredAmount(string enemyId)
    {
        foreach (var obj in currentQuest.objectives)
        {
            if (obj.targetId == enemyId)
                return obj.requiredAmount;
        }
        return 0;
    }

    void CheckQuestComplete()
    {
        if (questCompleted)
            return;

        foreach (var obj in currentQuest.objectives)
        {
            if (progress[obj.targetId] < obj.requiredAmount)
                return;
        }

        questCompleted = true;

        // Tính số sao dựa trên thời gian từ QuestData
        int stars = CalculateStars();
        int reward = GetRewardByStars(stars);
        
        // Lưu reward vào PlayerData
        SaveRewardToPlayerData(reward);
        
        Debug.Log($"Quest hoàn thành! Thời gian: {GetGameTimeFormatted()}, Số sao: {stars}, Reward: {reward}");

        Time.timeScale = 0f;

        UIManager.Instance.gamePlayPanel.ShowWinPanel(true, stars, reward);
        // Trigger next level, reward...
    }

    /// <summary>
    /// Tính số sao dựa trên thời gian hoàn thành quest
    /// </summary>
    /// <returns>Số sao đạt được (1-3)</returns>
    int CalculateStars()
    {
        if (currentQuest == null)
            return 1;

        float time = gameElapsedTime;

        // Nếu thời gian <= timeFor3Stars: 3 sao
        if (time <= currentQuest.timeFor3Stars)
        {
            return 3;
        }
        // Nếu thời gian <= timeFor2Stars: 2 sao
        else if (time <= currentQuest.timeFor2Stars)
        {
            return 2;
        }
        // Nếu thời gian > timeFor2Stars: 1 sao
        else
        {
            return 1;
        }
    }

    /// <summary>
    /// Lấy thời gian game đã trôi qua tính bằng giây
    /// </summary>
    public float GetGameTime()
    {
        return gameElapsedTime;
    }

    /// <summary>
    /// Lấy thời gian game đã trôi qua dưới dạng chuỗi định dạng (MM:SS)
    /// </summary>
    public string GetGameTimeFormatted()
    {
        int minutes = Mathf.FloorToInt(gameElapsedTime / 60f);
        int seconds = Mathf.FloorToInt(gameElapsedTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Reset thời gian game về 0
    /// </summary>
    public void ResetGameTime()
    {
        gameStartTime = Time.time;
        gameElapsedTime = 0f;
    }

    /// <summary>
    /// Lấy reward dựa trên số sao đạt được
    /// </summary>
    /// <param name="stars">Số sao (1-3)</param>
    /// <returns>Giá trị reward</returns>
    int GetRewardByStars(int stars)
    {
        if (currentQuest == null || currentQuest.rewardList == null)
            return 0;

        if (stars < 1 || stars > 3)
            return 0;

        // Index = stars - 1 (vì 1 sao -> index 0, 2 sao -> index 1, 3 sao -> index 2)
        if (stars - 1 < currentQuest.rewardList.Count)
        {
            return currentQuest.rewardList[stars - 1];
        }

        return 0;
    }

    /// <summary>
    /// Lưu reward vào PlayerData
    /// </summary>
    /// <param name="reward">Giá trị reward cần thêm</param>
    void SaveRewardToPlayerData(int reward)
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
        {
            PlayerDataManager.Instance.playerData.totalReward += reward;
            PlayerDataManager.Instance.Save();
            Debug.Log($"Đã nhận {reward} reward. Tổng reward: {PlayerDataManager.Instance.playerData.totalReward}");
        }
    }
}