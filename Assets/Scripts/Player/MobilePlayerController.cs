using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

/// <summary>
/// Script điều khiển player góc nhìn thứ 3 tối ưu cho mobile
/// Hỗ trợ virtual joystick cho di chuyển và touch để xoay camera
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MobilePlayerController : MonoBehaviour
{
    public static MobilePlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của player")]
    public float moveSpeed = 3f;
    
    [Tooltip("Tốc độ xoay nhân vật theo hướng di chuyển")]
    public float rotationSpeed = 10f;
    
    [Tooltip("Ngưỡng input để bắt đầu di chuyển (tránh drift)")]
    public float moveInputThreshold = 0.1f;

    [Header("Camera Settings")]
    [Tooltip("Transform của camera (nếu null sẽ tự động tìm Camera.main)")]
    public Transform cameraTransform;
    
    [Tooltip("Tốc độ nhạy cảm khi vuốt để xoay camera")]
    public float cameraSensitivity = 2f;
    
    [Tooltip("Giới hạn góc xoay dọc của camera (độ)")]
    public float cameraPitchMin = -40f;
    public float cameraPitchMax = 60f;
    
    [Tooltip("Vị trí offset của camera so với player")]
    public Vector3 cameraOffset = new Vector3(0f, 1.8f, -4f);
    
    [Tooltip("Thời gian smooth khi camera follow player")]
    public float cameraFollowSmoothTime = 0.1f;

    [Header("Shoot Settings")]
    [Tooltip("Arrow prefab để spawn khi bắn")]
    public GameObject arrowPrefab;
    
    [Tooltip("Vị trí spawn arrow")]
    public Transform arrowSpawnPoint;
    
    [Tooltip("Thời gian nhân vật dừng lại khi bắn (giây)")]
    public float shootStopDuration = 0.5f;
    
    [Tooltip("Thời gian hồi chiêu bắn (giây)")]
    public float shootCooldown = 2f;
    
    [Tooltip("Khoảng cách ray để tìm mục tiêu từ camera")]
    public float cameraAimRayDistance = 300f;

    [Header("Touch Controls")]
    [Tooltip("Cho phép xoay camera bằng touch (vuốt màn hình)")]
    public bool enableTouchCamera = true;
    
    [Tooltip("Vùng màn hình được dùng để xoay camera (0-1, từ trái sang phải)")]
    public Vector2 cameraTouchZone = new Vector2(0.5f, 1f); // Nửa màn hình bên phải
    
    [Tooltip("Tự động xoay nhân vật theo hướng camera khi xoay camera")]
    public bool rotatePlayerWithCamera = true;
    
    [Tooltip("Chỉ xoay nhân vật theo camera khi không có input di chuyển")]
    public bool rotateOnlyWhenNotMoving = false;

    [Header("Input Control")]
    [Tooltip("Cho phép nhận input từ người chơi hay không")]
    public bool canReceiveInput = true;

    // Private variables
    private CharacterController characterController;
    private PlayerAnimation playerAnimation;
    private Camera mainCamera;
    
    // Camera rotation
    private float cameraYaw = 0f;
    private float cameraPitch = 0f;
    private Vector3 cameraVelocity;
    
    // Movement
    private Vector2 moveInput = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;
    
    // Touch input
    private Vector2 lastTouchPosition = Vector2.zero;
    private bool isTouching = false;
    private int touchFingerId = -1;
    private bool isRotatingCamera = false; // Track if camera is being rotated
    
    // Shooting
    private bool isShooting = false;
    private float shootStopTimer = 0f;
    private float shootCooldownTimer = 0f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get components
        characterController = GetComponent<CharacterController>();
        playerAnimation = GetComponent<PlayerAnimation>();
        
        if (characterController == null)
        {
            Debug.LogError("MobilePlayerController: CharacterController component is missing!");
        }

        // Enable enhanced touch for mobile
        if (!EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Enable();
        }
    }

    private void Start()
    {
        // Setup camera
        if (cameraTransform == null && Camera.main != null)
        {
            mainCamera = Camera.main;
            cameraTransform = mainCamera.transform;
        }
        else if (cameraTransform != null)
        {
            mainCamera = cameraTransform.GetComponent<Camera>();
        }

        // Initialize camera rotation
        if (cameraTransform != null)
        {
            Vector3 angles = cameraTransform.eulerAngles;
            cameraYaw = angles.y;
            cameraPitch = angles.x;
            
            // Normalize pitch
            if (cameraPitch > 180f)
                cameraPitch -= 360f;
        }

        // Load player data if available
        if (PlayerDataManager.Instance != null)
        {
            moveSpeed = PlayerDataManager.Instance.playerData.speed / 10f;
        }
    }

    private void Update()
    {
        // Update timers
        UpdateShootTimers();

        // Check if can receive input
        if (!canReceiveInput)
        {
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        // Handle input
        HandleInput();
        
        // Handle camera rotation first (so we know if camera is being rotated)
        if (enableTouchCamera)
        {
            HandleTouchCamera();
        }
        else
        {
            HandleCamera();
        }
        
        // Handle movement (after camera to know if player should rotate with camera)
        HandleMovement();
        
        // Handle shooting
        if (!isShooting && shootCooldownTimer <= 0f)
        {
            HandleShootInput();
        }
    }

    private void LateUpdate()
    {
        // Update camera position (smooth follow)
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        // Get move input from InputManager
        if (InputManager.Instance != null)
        {
            moveInput = InputManager.Instance.InputMoveVector();
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    private void HandleMovement()
    {
        // Don't allow movement while shooting
        if (isShooting)
        {
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            characterController.Move(Physics.gravity * Time.deltaTime);
            return;
        }

        // Check if there's movement input
        bool hasMoveInput = moveInput.magnitude >= moveInputThreshold;

        if (hasMoveInput && cameraTransform != null)
        {
            // Calculate move direction relative to camera
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            // Project onto horizontal plane
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Calculate move direction
            moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

            // Rotate player towards move direction
            if (moveDirection.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // Apply movement
            Vector3 velocity = moveDirection * moveSpeed + Physics.gravity;
            characterController.Move(velocity * Time.deltaTime);

            // Update animation
            if (playerAnimation != null)
            {
                float normalizedSpeed = Mathf.Clamp01(moveInput.magnitude);
                playerAnimation.SetMovement(true, normalizedSpeed);
            }
        }
        else
        {
            // No movement input - rotate player with camera if enabled
            if (rotatePlayerWithCamera && cameraTransform != null)
            {
                // Only rotate if camera is being rotated, or if setting allows always rotating
                if (!rotateOnlyWhenNotMoving || isRotatingCamera)
                {
                    // Get camera forward direction on horizontal plane
                    Vector3 cameraForward = cameraTransform.forward;
                    cameraForward.y = 0f;
                    cameraForward.Normalize();

                    // Rotate player to face camera direction
                    if (cameraForward.magnitude > 0.01f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            targetRotation,
                            rotationSpeed * Time.deltaTime
                        );
                    }
                }
            }

            // No input - apply gravity only
            characterController.Move(Physics.gravity * Time.deltaTime);

            // Update animation
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
        }
    }

    private void HandleTouchCamera()
    {
        isRotatingCamera = false; // Reset flag
        
        // Handle touch input for camera rotation
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                // Check if touch is in camera zone (right side of screen)
                float screenX = touch.screenPosition.x / Screen.width;
                
                if (screenX >= cameraTouchZone.x && screenX <= cameraTouchZone.y)
                {
                    // This is camera rotation touch
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        lastTouchPosition = touch.screenPosition;
                        isTouching = true;
                        touchFingerId = touch.finger.index;
                    }
                    else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved && 
                             touch.finger.index == touchFingerId)
                    {
                        Vector2 deltaPosition = touch.screenPosition - lastTouchPosition;
                        
                        // Check if there's significant movement
                        if (deltaPosition.magnitude > 0.5f)
                        {
                            isRotatingCamera = true;
                        }
                        
                        // Rotate camera (deltaPosition is in pixels, scale by sensitivity)
                        // Divide by screen height to normalize, then multiply by sensitivity
                        float sensitivityScale = cameraSensitivity / Screen.height;
                        cameraYaw += deltaPosition.x * sensitivityScale;
                        cameraPitch -= deltaPosition.y * sensitivityScale;
                        
                        // Clamp pitch
                        cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
                        
                        lastTouchPosition = touch.screenPosition;
                    }
                    else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || 
                             touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                    {
                        if (touch.finger.index == touchFingerId)
                        {
                            isTouching = false;
                            touchFingerId = -1;
                            isRotatingCamera = false;
                        }
                    }
                }
            }
        }
        else
        {
            // Fallback to InputSystem Look if no touch
            if (!isTouching && InputManager.Instance != null)
            {
                Vector2 lookInput = InputManager.Instance.InputLookVector();
                if (lookInput.magnitude > 0.01f)
                {
                    isRotatingCamera = true;
                    cameraYaw += lookInput.x * cameraSensitivity * Time.deltaTime;
                    cameraPitch -= lookInput.y * cameraSensitivity * Time.deltaTime;
                    cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
                }
            }
        }
    }

    private void HandleCamera()
    {
        isRotatingCamera = false; // Reset flag
        
        // Use InputSystem Look input
        if (InputManager.Instance != null)
        {
            Vector2 lookInput = InputManager.Instance.InputLookVector();
            if (lookInput.magnitude > 0.01f)
            {
                isRotatingCamera = true;
                cameraYaw += lookInput.x * cameraSensitivity * Time.deltaTime;
                cameraPitch -= lookInput.y * cameraSensitivity * Time.deltaTime;
                cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
            }
        }
    }

    private void UpdateCameraPosition()
    {
        if (cameraTransform == null || characterController == null)
            return;

        // Calculate camera rotation
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        
        // Calculate desired position (offset from player)
        Vector3 targetPosition = transform.position + rotation * cameraOffset;
        
        // Smooth follow
        cameraTransform.position = Vector3.SmoothDamp(
            cameraTransform.position,
            targetPosition,
            ref cameraVelocity,
            cameraFollowSmoothTime
        );
        
        // Apply rotation
        cameraTransform.rotation = rotation;
    }

    private void HandleShootInput()
    {
        if (InputManager.Instance != null && InputManager.Instance.IsShooting())
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Set shooting state
        isShooting = true;
        shootStopTimer = shootStopDuration;
        shootCooldownTimer = shootCooldown;

        // Trigger animation
        if (playerAnimation != null)
        {
            playerAnimation.SetShoot();
        }

        // Spawn arrow
        SpawnArrow();
        
        // Update UI
        UpdateCooldownUI();
    }

    private void SpawnArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("MobilePlayerController: Arrow prefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = arrowSpawnPoint != null 
            ? arrowSpawnPoint.position 
            : transform.position + Vector3.up * 1.5f;

        // Calculate shoot direction (from camera center)
        Vector3 shootDirection = CalculateShootDirection(spawnPosition);

        // Instantiate arrow
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
        
        // Set direction
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        if (arrowComponent != null)
        {
            arrowComponent.SetDirection(shootDirection);
        }
        else
        {
            Debug.LogWarning("MobilePlayerController: Arrow prefab doesn't have Arrow component!");
        }
    }

    private Vector3 CalculateShootDirection(Vector3 spawnPosition)
    {
        if (mainCamera == null)
        {
            return transform.forward;
        }

        // Shoot from camera center
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, cameraAimRayDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(cameraAimRayDistance);
        }

        return (targetPoint - spawnPosition).normalized;
    }

    private void UpdateShootTimers()
    {
        // Update shoot stop timer
        if (isShooting)
        {
            shootStopTimer -= Time.deltaTime;
            if (shootStopTimer <= 0f)
            {
                isShooting = false;
            }
        }

        // Update cooldown timer
        if (shootCooldownTimer > 0f)
        {
            shootCooldownTimer = Mathf.Max(0f, shootCooldownTimer - Time.deltaTime);
        }
    }

    private void UpdateCooldownUI()
    {
        if (UIManager.Instance != null && UIManager.Instance.gamePlayPanel != null)
        {
            UIManager.Instance.gamePlayPanel.SetCountDown(shootCooldownTimer, shootCooldown);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Public methods for external control
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void SetCameraRotation(float yaw, float pitch)
    {
        cameraYaw = yaw;
        cameraPitch = Mathf.Clamp(pitch, cameraPitchMin, cameraPitchMax);
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
}
