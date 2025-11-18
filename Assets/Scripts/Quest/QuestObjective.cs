using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public QuestTargetType type;
    public string targetId; 
    public int requiredAmount;
}

public enum QuestTargetType
{
    KillAnimal,
    CollectItem
}
