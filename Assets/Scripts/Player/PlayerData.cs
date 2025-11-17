using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public List<PlayerLevelData> levels = new List<PlayerLevelData>();
    public List<QuestStatus> quests = new List<QuestStatus>();

    public static PlayerData CreateDefault(int totalLevels)
    {
        PlayerData data = new PlayerData();
        for (int i = 0; i < totalLevels; i++)
        {
            data.levels.Add(new PlayerLevelData
            {
                level = i + 1,
                star = 0,
                isLocked = i != 0
            });
        }
        data.quests.Add(new QuestStatus { questId = "Quest 1", completed = false });
        data.quests.Add(new QuestStatus { questId = "Quest 2", completed = false });
        data.quests.Add(new QuestStatus { questId = "Quest 3", completed = false });

        return data;
    }
}

[Serializable]
public class PlayerLevelData
{
    public int level;
    public int star;
    public bool isLocked;
}

[Serializable]
public class QuestStatus
{
    public string questId;
    public bool completed;
}