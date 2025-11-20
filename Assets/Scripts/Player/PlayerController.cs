using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject model;
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Look Rotation Settings")]
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float minPitch = -45f;
    [SerializeField] private float lookSensitivity = 0.15f;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform camTarget;
    
    [Header("Shoot")]
    [Tooltip("Thời gian nhân vật dừng lại khi bắn (giây)")]
    [SerializeField] private float shootStopDuration = 0.5f;
    [Tooltip("Thời gian hồi chiêu bắn (giây)")]
    [SerializeField] private float shootCooldown = 2f;
    
    [Header("Arrow")]
    [Tooltip("Arrow prefab để spawn khi bắn")]
    [SerializeField] private GameObject arrowPrefab;
    [Tooltip("Vị trí spawn arrow (Transform con của player, nếu null sẽ dùng vị trí player)")]
    [SerializeField] private Transform arrowSpawnPoint;
    [Tooltip("Khoảng cách ray để tìm mục tiêu từ camera")]
    [SerializeField] private float cameraAimRayDistance = 300f;
    [Tooltip("Layer mask cho ray aim (mặc định: tất cả)")]
    [SerializeField] private LayerMask cameraAimLayerMask = ~0;

    [Header("Input Control")]
    [Tooltip("Cho phép nhận input từ người chơi hay không")]
    [SerializeField] private bool canReceiveInput = true;
    [SerializeField] private bool isDisable = false;

    // Look rotation
    private Vector2 lookRotation = Vector2.zero;
    
    // Components
    private PlayerAnimation playerAnimation;
    private bool isShooting = false;
    private float shootStopTimer = 0f;
    private float shootCooldownTimer = 0f;
    private CameraController cameraController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Get components
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        
        playerAnimation = GetComponent<PlayerAnimation>();
        
        // Đảm bảo CharacterController tồn tại
        if (characterController == null)
        {
            Debug.LogError("PlayerController: CharacterController component is missing! Please add CharacterController to the player GameObject.");
        }
    }

    private void Start()
    {
        // Load move speed from PlayerDataManager if available
        if (PlayerDataManager.Instance != null)
        {
            moveSpeed = PlayerDataManager.Instance.playerData.speed / 10f;
        }
        
        // Setup camera
        SetupCamera();
        
        UpdateCooldownUI();
    }

    private void Update()
    {
        if (isDisable || !canReceiveInput)
        {
            // Nếu không cho phép input, dừng animation
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        HandleInput();
    }

    private void LateUpdate()
    {
        if (!isDisable)
        {
            UpdateCameraTarget();
        }
    }

    #region Initialization

    private void SetupCamera()
    {
        if (isDisable) return;
        
        if (camTarget != null)
        {
            camTarget.gameObject.SetActive(true);
        }
        
        // Tìm CameraController
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null && camTarget != null)
        {
            cameraController.SetTarget(camTarget);
        }
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (InputManager.Instance == null) return;

        HandleLookRotation();
        HandleMovement();
        HandleShootInput();
    }

    private void HandleLookRotation()
    {
        if (InputManager.Instance == null) return;

        Vector2 lookDelta = InputManager.Instance.InputLookVector();
        
        // Thêm look rotation với giới hạn pitch (đảo ngược dấu của pitch để kéo lên = nhìn lên)
        lookRotation.x -= lookDelta.y * lookSensitivity;
        lookRotation.x = Mathf.Clamp(lookRotation.x, minPitch, maxPitch);
        lookRotation.y += lookDelta.x * lookSensitivity;
        
        // Xoay player theo yaw (chỉ xoay trục Y)
        transform.rotation = Quaternion.Euler(0f, lookRotation.y, 0f);
    }

    private void HandleMovement()
    {
        if (characterController == null || InputManager.Instance == null)
        {
            return;
        }

        // Không cho phép di chuyển khi đang trong thời gian dừng vì bắn
        if (isShooting)
        {
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        Vector2 moveInput = InputManager.Instance.InputMoveVector();
        
        if (moveInput.magnitude < 0.1f)
        {
            // Không có input - chỉ áp dụng gravity
            characterController.Move(Physics.gravity * Time.deltaTime);
            
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        // Tính toán hướng di chuyển tương đối với camera rotation
        Vector3 worldDirection = GetWorldDirection(moveInput);
        
        // Di chuyển player
        Vector3 velocity = worldDirection * moveSpeed + Physics.gravity;
        characterController.Move(velocity * Time.deltaTime);

        // Cập nhật animation
        if (playerAnimation != null)
        {
            float moveSpeedValue = moveInput.magnitude;
            playerAnimation.SetMovement(true, moveSpeedValue);
        }
    }

    /// <summary>
    /// Chuyển đổi input direction sang world direction dựa trên camera rotation
    /// </summary>
    private Vector3 GetWorldDirection(Vector2 inputDirection)
    {
        // Lấy rotation từ camera hoặc từ player rotation
        Quaternion rotation = Quaternion.identity;
        
        if (Camera.main != null)
        {
            // Dùng camera yaw để tính hướng di chuyển
            float cameraYaw = Camera.main.transform.eulerAngles.y;
            rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        }
        else
        {
            // Nếu không có camera, dùng player rotation
            rotation = transform.rotation;
        }
        
        // Chuyển đổi input direction sang world direction
        Vector3 direction = new Vector3(inputDirection.x, 0f, inputDirection.y);
        return rotation * direction;
    }

    private void HandleShootInput()
    {
        if (isShooting || shootCooldownTimer > 0f) return;
        
        if (InputManager.Instance.IsShooting())
        {
            Shoot();
        }
    }

    #endregion

    #region Combat

    private void Shoot()
    {
        // Đặt trạng thái đang bắn và timer
        isShooting = true;
        shootStopTimer = shootStopDuration;
        shootCooldownTimer = shootCooldown;

        // Gọi PlayerAnimation để trigger animation bắn
        if (playerAnimation != null)
        {
            playerAnimation.SetShoot();
        }

        UpdateCooldownUI();

        // Spawn arrow
        SpawnArrow();
    }

    private void SpawnArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("PlayerController: Arrow prefab is not assigned!");
            return;
        }

        // Xác định vị trí spawn arrow
        Vector3 spawnPosition = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;

        Vector3 shootDirection = CalculateShootDirection(spawnPosition);

        // Tạo arrow
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

        // Gửi hướng cho mũi tên (arrow sẽ tự quay trong SetDirection)
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        if (arrowComponent != null)
        {
            arrowComponent.SetDirection(shootDirection);
        }
        else
        {
            Debug.LogWarning("PlayerController: Arrow prefab doesn't have Arrow component!");
        }
    }

    private Vector3 CalculateShootDirection(Vector3 spawnPosition)
    {
        if (Camera.main == null)
        {
            return transform.forward;
        }

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, cameraAimRayDistance, cameraAimLayerMask))
        {
            targetPoint = hit.point;   // điểm trúng
        }
        else
        {
            targetPoint = ray.GetPoint(cameraAimRayDistance); // nếu không trúng gì
        }

        return (targetPoint - spawnPosition).normalized;
    }

    #endregion

    #region Visual & Camera

    private void UpdateCameraTarget()
    {
        if (camTarget == null) return;
        
        // Xoay camTarget theo pitch (chỉ xoay trục X)
        camTarget.localRotation = Quaternion.Euler(lookRotation.x, 0f, 0f);
    }

    #endregion

    #region UI

    private void UpdateCooldownUI()
    {
        if (UIManager.Instance == null || UIManager.Instance.gamePlayPanel == null)
        {
            return;
        }

        UIManager.Instance.gamePlayPanel.SetCountDown(shootCooldownTimer, shootCooldown);
    }

    #endregion

    #region Update Timers

    private void FixedUpdate()
    {
        // Cập nhật timer bắn
        if (isShooting)
        {
            shootStopTimer -= Time.deltaTime;
            if (shootStopTimer <= 0f)
            {
                isShooting = false;
            }
        }

        // Cập nhật cooldown bắn
        if (shootCooldownTimer > 0f)
        {
            shootCooldownTimer = Mathf.Max(0f, shootCooldownTimer - Time.deltaTime);
            UpdateCooldownUI();
        }
    }

    #endregion

    #region Public Methods

    public void SetDisable(bool disable)
    {
        isDisable = disable;
        
        if (characterController != null)
        {
            characterController.enabled = !disable;
        }
        
        if (disable)
        {
            if (cameraController != null)
            {
                cameraController.SetTarget(null);
            }
            SetIdleAnimation();
        }
        else
        {
            SetupCamera();
        }
    }

    public void SetIdleAnimation()
    {
        // Set movement to idle (speed = 0)
        playerAnimation?.SetMovement(false, 0f);
    }

    /// <summary>
    /// Lấy look rotation hiện tại (pitch, yaw)
    /// </summary>
    public Vector2 GetLookRotation()
    {
        return lookRotation;
    }

    /// <summary>
    /// Set look rotation
    /// </summary>
    public void SetLookRotation(Vector2 rotation)
    {
        lookRotation.x = Mathf.Clamp(rotation.x, minPitch, maxPitch);
        lookRotation.y = rotation.y;
    }

    public GameObject GetModel()
    {
        return model;
    }

    #endregion
}
