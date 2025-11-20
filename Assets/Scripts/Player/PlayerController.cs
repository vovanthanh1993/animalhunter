using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Shoot")]
    [Tooltip("Phím bắn (mặc định: Space hoặc Click chuột trái)")]
    public Key shootKey = Key.Space;
    [Tooltip("Thời gian nhân vật dừng lại khi bắn (giây)")]
    public float shootStopDuration = 0.5f;
    [Tooltip("Thời gian hồi chiêu bắn (giây)")]
    public float shootCooldown = 2f;
    
    [Header("Arrow")]
    [Tooltip("Arrow prefab để spawn khi bắn")]
    public GameObject arrowPrefab;
    
    [Tooltip("Vị trí spawn arrow (Transform con của player, nếu null sẽ dùng vị trí player)")]
    public Transform arrowSpawnPoint;
    [Tooltip("Khoảng cách ray để tìm mục tiêu từ camera")]
    public float cameraAimRayDistance = 300f;
    [Tooltip("Layer mask cho ray aim (mặc định: tất cả)")]
    public LayerMask cameraAimLayerMask = ~0;

    [Header("Input Control")]
    [Tooltip("Cho phép nhận input từ người chơi hay không")]
    public bool canReceiveInput = true;

    [Header("Camera Alignment")]
    [Tooltip("Tự động xoay nhân vật theo hướng camera/Look input")]
    public bool alignWithLookInput = true;
    [Tooltip("Tốc độ xoay nhân vật (Slerp)")]
    public float lookRotationSpeed = 20f;
    [Tooltip("Ngưỡng look input để kích hoạt xoay")]
    public float lookInputThreshold = 0.05f;
    [Tooltip("Transform của camera dùng làm tham chiếu (nếu bỏ trống sẽ tìm Camera.main)")]
    public Transform cameraTransform;

    private PlayerAnimation playerAnimation;
    private bool isShooting = false;
    private float shootStopTimer = 0f;
    private float shootCooldownTimer = 0f;
    private CharacterController characterController;
    private Vector3 lastMoveDirection = Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerAnimation = GetComponent<PlayerAnimation>();
        characterController = GetComponent<CharacterController>();
        
        // Đảm bảo CharacterController tồn tại
        if (characterController == null)
        {
            Debug.LogError("PlayerController: CharacterController component is missing! Please add CharacterController to the player GameObject.");
        }
        
        UpdateCooldownUI();
    }

    void Start()
    {
        moveSpeed = PlayerDataManager.Instance.playerData.speed/10;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
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
        }

        UpdateCooldownUI();

        // Kiểm tra xem có cho phép nhận input không
        if (!canReceiveInput)
        {
            // Nếu không cho phép input, dừng animation
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        HandleMovement();

        // Xử lý input bắn (chỉ cho phép bắn khi không đang bắn và cooldown đã hết)
        if (!isShooting && shootCooldownTimer <= 0f)
        {
            HandleShootInput();
        }
    }

    private void HandleShootInput()
    {
        if (InputManager.Instance.IsShooting())
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Đặt trạng thái đang bắn và timer
        isShooting = true;
        shootStopTimer = shootStopDuration;
        
        // Reset cooldown timer về 2 giây
        shootCooldownTimer = shootCooldown;

        // Gọi PlayerAnimation để trigger animation bắn
        if (playerAnimation != null)
        {
            playerAnimation.SetShoot();
        }

        UpdateCooldownUI();

        // Spawn arrow
        Vector3 shootDirection = SpawnArrow();
    }

    private Vector3 SpawnArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("PlayerController: Arrow prefab is not assigned!");
            return transform.forward;
        }

        // Xác định vị trí spawn arrow
        Vector3 spawnPosition = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;

        Vector3 shootDirection3D = CalculateShootDirection(spawnPosition);

        // Tạo arrow
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

        // Gửi hướng cho mũi tên (arrow sẽ tự quay trong SetDirection)
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        if (arrowComponent != null)
        {
            arrowComponent.SetDirection(shootDirection3D);
        }
        else
        {
            Debug.LogWarning("PlayerController: Arrow prefab doesn't have Arrow component!");
        }

        return shootDirection3D;
    }

    private void UpdateCooldownUI()
    {
        if (UIManager.Instance == null || UIManager.Instance.gamePlayPanel == null)
        {
            return;
        }

        UIManager.Instance.gamePlayPanel.SetCountDown(shootCooldownTimer, shootCooldown);
    }

    private void AlignWithLookInput(bool isMoving, Vector3 moveDirection)
    {
        if (InputManager.Instance == null)
        {
            return;
        }

        Vector2 lookInput = InputManager.Instance.InputLookVector();
        bool hasLookInput = lookInput.sqrMagnitude > lookInputThreshold * lookInputThreshold;

        Vector3 targetForward;

        if (hasLookInput && cameraTransform != null)
        {
            targetForward = cameraTransform.forward;
            targetForward.y = 0f;
        }
        else if (isMoving && moveDirection.sqrMagnitude > 0.0001f)
        {
            targetForward = moveDirection;
        }
        else
        {
            return;
        }

        if (targetForward.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetForward.normalized);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            lookRotationSpeed * Time.deltaTime);
    }

    private Vector3 CalculateShootDirection(Vector3 spawnPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, cameraAimRayDistance))
        {
            targetPoint = hit.point;   // điểm trúng
        }
        else
        {
            targetPoint = ray.GetPoint(cameraAimRayDistance); // nếu không trúng gì
        }

        return (targetPoint - spawnPosition).normalized;
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
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        bool hasInput = inputDirection.magnitude >= 0.1f;

        if (hasInput)
        {
            float referenceY = cameraTransform != null
                ? cameraTransform.eulerAngles.y
                : transform.eulerAngles.y;

            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + referenceY;
            float smoothedAngle = Mathf.LerpAngle(
                transform.eulerAngles.y,
                targetAngle,
                lookRotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            lastMoveDirection = moveDir;

            Vector3 velocity = moveDir * moveSpeed + Physics.gravity;
            characterController.Move(velocity * Time.deltaTime);

            if (playerAnimation != null)
            {
                float normalizedSpeed = Mathf.Clamp01(moveInput.magnitude);
                playerAnimation.SetMovement(true, normalizedSpeed);
            }
        }
        else
        {
            characterController.Move(Physics.gravity * Time.deltaTime);

            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
        }
    }

}
