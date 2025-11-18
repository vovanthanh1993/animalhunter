using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    public float speed = 20f;
    public float lifeTime = 1f;
    public float damage = 10f;
    
    [Header("Top-Down Rotation Settings")]
    public float fixedRotationY = 0f;
    public Vector3 rotationOffset = Vector3.zero;
    public bool updateRotationWhileFlying = true;
    
    private Vector2 direction;
    private Vector3 direction3D;
    private bool isDestroyed = false;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        direction3D = new Vector3(direction.x, 0f, direction.y);
        UpdateRotation();
    }
    
    private void UpdateRotation()
    {
        if (direction.sqrMagnitude < 0.001f) return;
        
        float angleZ = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        
        Vector3 finalRotation = new Vector3(
            -90f + rotationOffset.x,
            fixedRotationY,
            angleZ + rotationOffset.z
        );
        
        transform.rotation = Quaternion.Euler(finalRotation);
    }

    void Update()
    {
        if (direction.sqrMagnitude < 0.001f) return;
        
        transform.Translate(direction3D * speed * Time.deltaTime, Space.World);
        
        if (updateRotationWhileFlying)
        {
            UpdateRotation();
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
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

