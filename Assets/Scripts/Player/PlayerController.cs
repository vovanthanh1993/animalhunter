using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Shoot")]
    [Tooltip("Phím bắn (mặc định: Space hoặc Click chuột trái)")]
    public Key shootKey = Key.Space;
    [Tooltip("Thời gian nhân vật dừng lại khi bắn (giây)")]
    public float shootStopDuration = 0.5f;
    [Tooltip("Thời gian hồi chiêu bắn (giây)")]
    public float shootCooldown = 2f;

    private PlayerAnimation playerAnimation;
    private bool isShooting = false;
    private float shootStopTimer = 0f;
    private float shootCooldownTimer = 0f;

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
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
            shootCooldownTimer -= Time.deltaTime;
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
        if (isMoving && !isShooting)
        {
            // Di chuyển trên mặt phẳng XZ (top-down)
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Quay nhân vật theo hướng di chuyển
            transform.forward = direction;
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
        // Kiểm tra phím bắn (Space hoặc phím được chỉ định)
        bool shootPressed = Keyboard.current != null && 
                           (Keyboard.current[shootKey].wasPressedThisFrame || 
                            Keyboard.current[Key.Space].wasPressedThisFrame);

        // Hoặc click chuột trái
        bool mouseShoot = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (shootPressed || mouseShoot)
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

        // TODO: Thêm logic bắn đạn ở đây
        // Ví dụ: Instantiate bullet, raycast, v.v.
    }
}
