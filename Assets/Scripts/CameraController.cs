
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Target")]
    public Transform cameraTarget;
    public Vector3 followOffset = new Vector3(0f, 1.8f, -4f);
    public float followSmoothTime = 0.05f;

    [Header("Sensitivity")]
    public float sensitivity = 120f;

    [Header("Clamp Angle")]
    public float minPitch = -40f;
    public float maxPitch = 60f;

    private float yaw;
    private float pitch;
    private Vector3 followVelocity;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void LateUpdate()
    {
        if (cameraTarget == null)
        {
            return;
        }

        Vector2 lookInput = InputLookVector();

        yaw += lookInput.x * sensitivity * Time.deltaTime;
        pitch -= lookInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = cameraTarget.position + rotation * followOffset;

        transform.rotation = rotation;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followSmoothTime);
    }

    public Vector2 InputLookVector()
    {
        if (InputManager.Instance == null)
        {
            return Vector2.zero;
        }

        return InputManager.Instance.InputLookVector();
    }
}