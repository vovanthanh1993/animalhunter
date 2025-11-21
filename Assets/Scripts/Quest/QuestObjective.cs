using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public QuestTargetType type;
    public EnemyType enemyType; 
    public int requiredAmount;
}

public enum QuestTargetType
{
    KillAnimal,
    CollectItem
}
