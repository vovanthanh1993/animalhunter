using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    public float speed = 20f;
    public float lifeTime = 1f;
    public float damage = 10f;
    public bool alignWithMovement = true;

    private Vector3 direction = Vector3.zero;
    private bool isDestroyed = false;

    void Start()
    {
        // Lấy damage từ PlayerData
        LoadDamageFromPlayerData();
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Load damage từ PlayerData và gán vào damage của arrow
    /// </summary>
    void LoadDamageFromPlayerData()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
        {
            damage = PlayerDataManager.Instance.playerData.damage;
        }
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;

        if (direction.sqrMagnitude < 0.0001f || !alignWithMovement)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;
    }

    void Update()
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;
        
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
        
        isDestroyed = true;
        Destroy(gameObject);
    }
}

