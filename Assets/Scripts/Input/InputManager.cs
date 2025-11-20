using UnityEngine;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputSystem_Actions InputSystem;

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
        return InputSystem.Player.Look.ReadValue<Vector2>();
    }
}