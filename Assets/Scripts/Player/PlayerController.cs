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

    [Header("Input Control")]
    [Tooltip("Cho phép nhận input từ người chơi hay không")]
    public bool canReceiveInput = true;

    private PlayerAnimation playerAnimation;
    private bool isShooting = false;
    private float shootStopTimer = 0f;
    private float shootCooldownTimer = 0f;
    private CharacterController characterController;

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

        // Lấy input từ InputManager (Input System mới)
        Vector2 moveInput = Vector2.zero;

        if (InputManager.Instance != null && InputManager.Instance.InputSystem != null)
        {
            // Đọc giá trị Vector2 từ action Move trong Input System
            moveInput = InputManager.Instance.InputMoveVector();
        }

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        bool isMoving = direction.sqrMagnitude > 0.001f && !isShooting; // Không di chuyển khi đang bắn
        float speed = moveInput.magnitude;

        // Chỉ cho phép di chuyển nếu không đang bắn
        if (isMoving && !isShooting && characterController != null)
        {
            // Tính toán hướng di chuyển
            Vector3 moveDirection = direction * moveSpeed;
            
            // CharacterController tự động xử lý collision
            // Move() sẽ không di chuyển nếu có vật cản
            characterController.Move(moveDirection * Time.deltaTime);

            // Quay nhân vật theo hướng di chuyển
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.forward = direction;
            }
        }

        // Gọi PlayerAnimation để cập nhật animation
        if (playerAnimation != null)
        {
            playerAnimation.SetMovement(isMoving, speed);
        }

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
        Vector3 spawnPosition;
        if (arrowSpawnPoint != null)
        {
            spawnPosition = arrowSpawnPoint.position;
        }
        else
        {
            // Spawn ở phía trước player
            spawnPosition = transform.position + transform.forward * 1f + Vector3.up * 1f;
        }

        // Lấy hướng bắn từ transform.forward của player (hướng player đang nhìn)
        Vector3 shootDirection3D = transform.forward;
        
        // Chuyển đổi từ Vector3 sang Vector2 (x, z) cho top-down game
        Vector2 shootDirection = new Vector2(shootDirection3D.x, shootDirection3D.z);

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

    private void UpdateCooldownUI()
    {
        if (UIManager.Instance == null || UIManager.Instance.gamePlayPanel == null)
        {
            return;
        }

        UIManager.Instance.gamePlayPanel.SetCountDown(shootCooldownTimer, shootCooldown);
    }
}
