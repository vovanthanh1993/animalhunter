using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float runSpeed = 5f;
    public float runDistance = 5f;
    
    [Header("Player Detection")]
    public float detectionDistance = 3f;
    public float checkInterval = 0.1f;
    
    private NavMeshAgent navMeshAgent;
    private EnemyAnimation enemyAnimation;
    private bool isRunning = false;
    private float lastCheckTime = 0f;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        navMeshAgent.speed = runSpeed;
        navMeshAgent.stoppingDistance = 0.1f;
        
        enemyAnimation = GetComponent<EnemyAnimation>();
    }

    void Update()
    {
        CheckPlayerDistance();
        
        if (isRunning && navMeshAgent != null)
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.1f)
            {
                StopRunning();
            }
        }
    }

    private void CheckPlayerDistance()
    {
        if (Time.time - lastCheckTime < checkInterval) return;
        lastCheckTime = Time.time;
        
        if (PlayerController.Instance == null) return;
        
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        Vector3 enemyPosition = transform.position;
        float distance = Vector3.Distance(new Vector3(playerPosition.x, 0f, playerPosition.z), 
                                          new Vector3(enemyPosition.x, 0f, enemyPosition.z));
        
        if (distance <= detectionDistance && !isRunning)
        {
            RunAwayFromPlayer();
        }
    }

    private void RunAwayFromPlayer()
    {
        if (navMeshAgent == null || PlayerController.Instance == null) return;
        
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        Vector3 enemyPosition = transform.position;
        Vector3 directionAway = (enemyPosition - playerPosition).normalized;
        directionAway.y = 0f;
        
        Vector3 targetPosition = enemyPosition + directionAway * runDistance;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, runDistance, NavMesh.AllAreas))
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(hit.position);
            isRunning = true;
            
            if (enemyAnimation != null)
            {
                enemyAnimation.SetRun(true);
            }
        }
    }

    public void RunAwayRandom()
    {
        if (navMeshAgent == null) return;
        
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 runDirection = new Vector3(Mathf.Sin(randomAngle), 0f, Mathf.Cos(randomAngle));
        Vector3 targetPosition = transform.position + runDirection * runDistance;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, runDistance, NavMesh.AllAreas))
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(hit.position);
            isRunning = true;
            
            if (enemyAnimation != null)
            {
                enemyAnimation.SetRun(true);
            }
        }
    }

    public void StopRunning()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
        isRunning = false;
        
        if (enemyAnimation != null)
        {
            enemyAnimation.SetRun(false);
        }
    }
}
