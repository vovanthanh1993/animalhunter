using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Run Settings")]
    public float runDuration = 1f;
    
    [Header("Death Settings")]
    public float dieDelay = 1f;
    
    private EnemyAnimation enemyAnimation;
    private EnemyController enemyController;
    private Coroutine runCoroutine;
    private bool isDead = false;

    public string enemyId;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAnimation = GetComponent<EnemyAnimation>();
        enemyController = GetComponent<EnemyController>();
        
        if (enemyAnimation != null)
        {
            enemyAnimation.SetRun(false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
            return;
        }
        
        if (enemyController != null)
        {
            enemyController.RunAwayRandom();
        }
        
        if (enemyAnimation != null)
        {
            if (runCoroutine != null)
            {
                StopCoroutine(runCoroutine);
            }
            runCoroutine = StartCoroutine(RunForDuration());
        }
    }

    private IEnumerator RunForDuration()
    {
        if (enemyAnimation != null)
        {
            enemyAnimation.SetRun(true);
        }
        
        yield return new WaitForSeconds(runDuration);
        
        if (enemyController != null)
        {
            enemyController.StopRunning();
        }
        
        if (enemyAnimation != null)
        {
            enemyAnimation.SetRun(false);
        }
        
        runCoroutine = null;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (runCoroutine != null)
        {
            StopCoroutine(runCoroutine);
        }
        
        if (enemyAnimation != null)
        {
            enemyAnimation.SetDie();
        }
        
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(dieDelay);
        Destroy(gameObject);
        QuestManager.Instance.OnEnemyKilled(enemyId);
    }
}
