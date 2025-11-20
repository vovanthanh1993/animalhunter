using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    
    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float rotationSmoothSpeed = 6f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private bool useDamping = true;
    [SerializeField] private float dampingFactor = 0.95f;
    
    [Header("Obstacle Avoidance")]
    [SerializeField] private bool enableObstacleAvoidance = true;
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    [SerializeField] private float obstacleCheckRadius = 0.5f;
    [SerializeField] private float minDistanceFromObstacle = 1f;
    
    [Header("Camera Angles")]
    [SerializeField] private Vector3 angle1Position = new Vector3(0, 1.5f, -3f);
    [SerializeField] private Vector3 angle1Rotation = new Vector3(15, 0, 0);
    
    [SerializeField] private Vector3 angle2Position = new Vector3(0, 3f, -5f);
    [SerializeField] private Vector3 angle2Rotation = new Vector3(25, 0, 0);
    
    [SerializeField] private Vector3 angle3Position = new Vector3(0, 5f, -8f);
    [SerializeField] private Vector3 angle3Rotation = new Vector3(35, 0, 0);
    
    [Header("Current Settings")]
    [SerializeField] private int currentAngle = 1;
    
    private Transform _target;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private Vector3 _currentVelocity;
    private Vector3 _currentAngularVelocity;
    private Vector3 _lastTargetPosition;
    private Quaternion _lastTargetRotation;
    
    public enum CameraAngle
    {
        Angle1 = 1,
        Angle2 = 2,
        Angle3 = 3
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetTarget(Transform followTarget)
    {
        _target = followTarget;
        UpdateCameraPosition();
    }
    
    public void SwitchToAngle(int angle)
    {
        currentAngle = Mathf.Clamp(angle, 1, 3);
        UpdateCameraPosition();
    }
    
    public void SwitchToAngle(CameraAngle angle)
    {
        currentAngle = (int)angle;
        UpdateCameraPosition();
    }
    
    public void NextAngle()
    {
        currentAngle = currentAngle % 3 + 1;
        UpdateCameraPosition();
    }
    
    public void PreviousAngle()
    {
        currentAngle = currentAngle == 1 ? 3 : currentAngle - 1;
        UpdateCameraPosition();
    }
    
    private void UpdateCameraPosition()
    {
        if (_target == null) return;
        
        Vector3 basePosition = _target.position;
        Vector3 offsetPosition;
        Vector3 offsetRotation;
        
        switch (currentAngle)
        {
            case 1:
                offsetPosition = angle1Position;
                offsetRotation = angle1Rotation;
                break;
            case 2:
                offsetPosition = angle2Position;
                offsetRotation = angle2Rotation;
                break;
            case 3:
                offsetPosition = angle3Position;
                offsetRotation = angle3Rotation;
                break;
            default:
                offsetPosition = angle1Position;
                offsetRotation = angle1Rotation;
                break;
        }
        
        // Calculate target position relative to player
        Vector3 desiredPosition = basePosition + _target.rotation * offsetPosition;
        
        // Kiểm tra và tránh vật cản
        if (enableObstacleAvoidance)
        {
            _targetPosition = CheckForObstacles(basePosition, desiredPosition);
        }
        else
        {
            _targetPosition = desiredPosition;
        }
        
        _targetRotation = _target.rotation * Quaternion.Euler(offsetRotation);
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        
        // Store last target position for velocity calculation
        _lastTargetPosition = _targetPosition;
        _lastTargetRotation = _targetRotation;
        
        // Update camera position if target moved
        UpdateCameraPosition();
        
        // Calculate target velocity
        Vector3 targetVelocity = (_targetPosition - _lastTargetPosition) / Time.deltaTime;
        
        // Smooth movement with acceleration/deceleration
        if (Vector3.Distance(transform.position, _targetPosition) > 0.01f)
        {
            // Accelerate towards target
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, acceleration * Time.deltaTime);
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _currentVelocity, 1f / smoothSpeed);
        }
        else
        {
            // Decelerate when close to target
            _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }
        
        // Smooth rotation
        if (Quaternion.Angle(transform.rotation, _targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
        
        // Apply damping to reduce jitter
        if (useDamping)
        {
            _currentVelocity *= dampingFactor;
        }
    }
    
    public int GetCurrentAngle()
    {
        return currentAngle;
    }
    
    public CameraAngle GetCurrentCameraAngle()
    {
        return (CameraAngle)currentAngle;
    }
    
    public void ResetCameraSmoothness()
    {
        _currentVelocity = Vector3.zero;
        _currentAngularVelocity = Vector3.zero;
    }
    
    public void SetSmoothness(float positionSmooth, float rotationSmooth)
    {
        smoothSpeed = positionSmooth;
        rotationSmoothSpeed = rotationSmooth;
    }
    
    private Vector3 CheckForObstacles(Vector3 playerPosition, Vector3 desiredCameraPosition)
    {
        Vector3 direction = desiredCameraPosition - playerPosition;
        float distance = direction.magnitude;
        
        // Raycast từ player đến vị trí camera mong muốn
        RaycastHit hit;
        if (Physics.SphereCast(playerPosition, obstacleCheckRadius, direction.normalized, out hit, distance, obstacleLayerMask))
        {
            // Nếu có vật cản, di chuyển camera gần player hơn
            Vector3 adjustedPosition = playerPosition + direction.normalized * (hit.distance - minDistanceFromObstacle);
            
            // Đảm bảo camera không quá gần player
            float minDistance = 1f;
            if (Vector3.Distance(adjustedPosition, playerPosition) < minDistance)
            {
                adjustedPosition = playerPosition + direction.normalized * minDistance;
            }
            
            return adjustedPosition;
        }
        
        return desiredCameraPosition;
    }
} 