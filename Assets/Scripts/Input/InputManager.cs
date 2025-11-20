using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputSystem_Actions InputSystem;
    
    [Header("Look Input Settings")]
    [Tooltip("Chỉ cho phép Look input từ pointer (mouse/touch) khi ở nửa màn hình bên phải")]
    public bool restrictLookToRightHalf = true;

    private void OnEnable()
    {
        
    }

    /*private void OnDisable()
    {
        InputSystem.Disable();
    }*/

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InputSystem = new InputSystem_Actions();
            InputSystem.Enable();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DisablePlayerInput()
    {
        InputSystem.Player.Disable();
    }

    public Vector2 InputMoveVector()
    {
        return InputSystem.Player.Move.ReadValue<Vector2>();
    }

    public bool IsShooting()
    {
        return InputSystem.Player.Attack.triggered;
    }

    public Vector2 InputLookVector()
    {
        Vector2 lookInput = InputSystem.Player.Look.ReadValue<Vector2>();
        
        // Nếu không restrict, trả về input bình thường
        if (!restrictLookToRightHalf)
        {
            return lookInput;
        }
        
        // Kiểm tra xem có input không
        if (lookInput.magnitude > 0.01f)
        {
            // Kiểm tra xem có mouse hoặc touch đang hoạt động không
            bool hasMouse = Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > 0.01f;
            bool hasTouch = Touchscreen.current != null && Touchscreen.current.touches.Count > 0;
            
            // Nếu có pointer input (mouse hoặc touch), kiểm tra vị trí
            if (hasMouse || hasTouch)
            {
                Vector2 pointerPosition = GetPointerPosition();
                
                // Nếu lấy được vị trí pointer, kiểm tra vùng
                if (pointerPosition != Vector2.zero)
                {
                    float screenX = pointerPosition.x / Screen.width;
                    if (screenX >= 0.5f)
                    {
                        // Pointer ở nửa màn hình bên phải - cho phép input
                        return lookInput;
                    }
                    else
                    {
                        // Pointer ở nửa màn hình bên trái - chặn input
                        return Vector2.zero;
                    }
                }
            }
            // Nếu không có pointer input (gamepad, joystick), cho phép input
        }
        
        // Trả về input
        return lookInput;
    }
    
    /// <summary>
    /// Kiểm tra xem control có phải là pointer (mouse/touch) không
    /// </summary>
    private bool IsPointerControl(InputControl control)
    {
        if (control == null) return false;
        
        // Kiểm tra device của control
        InputDevice device = control.device;
        
        // Kiểm tra mouse
        if (device is Mouse)
        {
            return true;
        }
        
        // Kiểm tra touchscreen
        if (device is Touchscreen)
        {
            return true;
        }
        
        // Kiểm tra tên control path
        string controlPath = control.path;
        if (controlPath != null)
        {
            // Kiểm tra các pattern phổ biến của pointer
            if (controlPath.Contains("<Pointer>") || 
                controlPath.Contains("/delta") ||
                controlPath.Contains("Mouse") ||
                controlPath.Contains("Touch"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Lấy vị trí pointer hiện tại (mouse hoặc touch)
    /// </summary>
    private Vector2 GetPointerPosition()
    {
        // Kiểm tra mouse trước
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            // Kiểm tra xem mouse có đang di chuyển không
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.magnitude > 0.01f || mousePos.magnitude > 0f)
            {
                return mousePos;
            }
        }
        
        // Nếu không có mouse, kiểm tra touch
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            if (touches.Count > 0)
            {
                // Tìm touch đang di chuyển hoặc đang được nhấn
                for (int i = 0; i < touches.Count; i++)
                {
                    var touch = touches[i];
                    var delta = touch.delta.ReadValue();
                    var phase = touch.phase.ReadValue();
                    var position = touch.position.ReadValue();
                    
                    // Nếu touch đang di chuyển hoặc đang được nhấn
                    if (delta.magnitude > 0.01f || 
                        phase == UnityEngine.InputSystem.TouchPhase.Moved || 
                        phase == UnityEngine.InputSystem.TouchPhase.Stationary ||
                        phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        return position;
                    }
                }
                
                // Nếu không có touch đang di chuyển, lấy touch đầu tiên
                if (touches.Count > 0)
                {
                    return touches[0].position.ReadValue();
                }
            }
        }
        
        return Vector2.zero;
    }
}