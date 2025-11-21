using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestData currentQuest;
    
    public Dictionary<EnemyType, int> progress = new Dictionary<EnemyType, int>();

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
            // Chỉ khởi tạo progress cho objectives có type KillAnimal
            if (obj.type == QuestTargetType.KillAnimal)
            {
                progress[obj.enemyType] = 0;
            }
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

    public void OnEnemyKilled(EnemyType enemyType)
    {
        if (!progress.ContainsKey(enemyType))
            return;

        progress[enemyType]++;

        Debug.Log($"{enemyType} progress: {progress[enemyType]} / {GetRequiredAmount(enemyType)}");

        CheckQuestComplete();
    }

    int GetRequiredAmount(EnemyType enemyType)
    {
        foreach (var obj in currentQuest.objectives)
        {
            if (obj.type == QuestTargetType.KillAnimal && obj.enemyType == enemyType)
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
            // Chỉ check objectives có type KillAnimal
            if (obj.type == QuestTargetType.KillAnimal)
            {
                if (!progress.ContainsKey(obj.enemyType) || progress[obj.enemyType] < obj.requiredAmount)
                    return;
            }
        }

        questCompleted = true;

        // Tính số sao dựa trên thời gian từ QuestData
        int stars = CalculateStars();
        int reward = GetRewardByStars(stars);
        
        // Lưu reward vào PlayerData
        SaveRewardToPlayerData(reward);
        
        // Lưu số sao vào PlayerLevelData
        SaveStarsToLevelData(stars);
        
        Debug.Log($"Quest hoàn thành! Thời gian: {GetGameTimeFormatted()}, Số sao: {stars}, Reward: {reward}");

        Time.timeScale = 0f;

        UIManager.Instance.gamePlayPanel.ShowWinPanel(true, stars, reward);
        // Trigger next level, reward...
    }
    
    /// <summary>
    /// Lấy số level từ tên scene (ví dụ: "Level1" -> 1, "Level2" -> 2)
    /// </summary>
    int GetCurrentLevelFromScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        // Kiểm tra nếu scene name bắt đầu bằng "Level"
        if (sceneName.StartsWith("Level"))
        {
            // Lấy phần số sau "Level"
            string levelStr = sceneName.Substring(5); // Bỏ qua "Level"
            
            if (int.TryParse(levelStr, out int level))
            {
                return level;
            }
        }
        
        // Fallback: trả về 1 nếu không parse được
        Debug.LogWarning($"QuestManager: Không thể parse level từ scene name: {sceneName}");
        return 1;
    }
    
    /// <summary>
    /// Lưu số sao vào PlayerLevelData và unlock level tiếp theo
    /// </summary>
    /// <param name="stars">Số sao đạt được (1-3)</param>
    void SaveStarsToLevelData(int stars)
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.playerData == null)
        {
            Debug.LogWarning("QuestManager: Cannot save stars - PlayerDataManager is null");
            return;
        }
        
        int currentLevel = GetCurrentLevelFromScene();
        Debug.Log($"Current level: {currentLevel}");
        PlayerData playerData = PlayerDataManager.Instance.playerData;
        
        // Tìm level data tương ứng với currentLevel
        PlayerLevelData levelData = playerData.levels.Find(l => l.level == currentLevel);
        
        if (levelData != null)
        {
            // Chỉ cập nhật nếu số sao mới cao hơn số sao cũ
            if (stars > levelData.star)
            {
                levelData.star = stars;
                Debug.Log($"Đã lưu {stars} sao cho Level {currentLevel}");
            }
            else
            {
                Debug.Log($"Level {currentLevel} đã có {levelData.star} sao, không cập nhật với {stars} sao");
            }
            
            // Unlock level tiếp theo nếu chưa unlock
            int nextLevel = currentLevel + 1;
            PlayerLevelData nextLevelData = playerData.levels.Find(l => l.level == nextLevel);
            
            if (nextLevelData != null && nextLevelData.isLocked)
            {
                nextLevelData.isLocked = false;
                Debug.Log($"Đã unlock Level {nextLevel}");
            }
            
            // Lưu dữ liệu
            PlayerDataManager.Instance.Save();
        }
        else
        {
            Debug.LogWarning($"QuestManager: Không tìm thấy Level {currentLevel} trong PlayerData");
        }
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