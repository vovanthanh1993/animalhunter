using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private float _arrowSpeed = 20f;
    [SerializeField] private float _arrowLifetime = 1f;
    [SerializeField] private float _arrowDamage = 10f;

    public Vector3 direction;
    private bool _isDestroyed = false;

    private void Start() {
        LoadDamageFromPlayerData();
        Destroy(gameObject, _arrowLifetime);
    }

    /// <summary>
    /// Load damage từ PlayerData và gán vào damage của arrow
    /// </summary>
    private void LoadDamageFromPlayerData()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
        {
            _arrowDamage = PlayerDataManager.Instance.playerData.damage;
        }
    }

    void Update()
    {
        transform.position += direction * _arrowSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isDestroyed) return;
        
        // Deal damage to enemy
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(_arrowDamage);
        }
        
        _isDestroyed = true;
        Destroy(gameObject);
    }
}

