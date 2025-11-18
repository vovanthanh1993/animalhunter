using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          // Nhân vật cần camera theo dõi

    [Header("Camera Settings")]
    [Tooltip("Góc nghiêng camera theo trục X (độ)")]
    public float angleX = 45f;
    
    [Tooltip("Góc xoay camera theo trục Y (độ)")]
    public float angleY = 45f;
    
    [Tooltip("Khoảng cách camera từ player")]
    public float distance = 15f;
    
    [Tooltip("Offset vị trí camera so với player")]
    public Vector3 offset = Vector3.zero;

    [Header("Follow Settings")]
    [Tooltip("Tốc độ camera đuổi theo target")]
    public float followSpeed = 10f;
    
    [Tooltip("Ngưỡng khoảng cách tối thiểu để coi là player đang di chuyển (mét)")]
    public float positionThreshold = 0.01f;

    private Vector3 lastTargetPosition;

    private void Start()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
            // Khởi tạo vị trí camera ngay từ đầu
            UpdateCameraPosition(target.position);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 currentTargetPosition = target.position;
        
        // Chỉ kiểm tra di chuyển trên mặt phẳng XZ (bỏ qua trục Y)
        Vector3 lastPosXZ = new Vector3(lastTargetPosition.x, 0f, lastTargetPosition.z);
        Vector3 currentPosXZ = new Vector3(currentTargetPosition.x, 0f, currentTargetPosition.z);
        float positionDelta = Vector3.Distance(currentPosXZ, lastPosXZ);
        bool isPlayerMoving = positionDelta > positionThreshold;

        if (isPlayerMoving)
        {
            // Chỉ di chuyển camera khi player thực sự di chuyển trên mặt phẳng
            UpdateCameraPosition(currentTargetPosition);
            lastTargetPosition = currentTargetPosition;
        }
        else
        {
            // Khi player bị cản, camera không di chuyển nhưng vẫn nhìn về target
            UpdateCameraRotation(currentTargetPosition);
        }
    }

    private void UpdateCameraPosition(Vector3 targetPosition)
    {
        // Tính toán vị trí camera dựa trên góc x=45, y=45
        // Tạo rotation từ góc X và Y
        Quaternion rotation = Quaternion.Euler(angleX, angleY, 0f);
        
        // Tính toán offset từ rotation và distance
        // Camera sẽ ở phía sau và trên player
        Vector3 direction = rotation * Vector3.back; // Vector3.back = (0, 0, -1)
        Vector3 cameraPosition = targetPosition + direction * distance + offset;

        // Di chuyển camera mượt mà đến vị trí mong muốn
        transform.position = Vector3.Lerp(transform.position, cameraPosition, followSpeed * Time.deltaTime);

        // Cập nhật rotation để camera nhìn về player
        UpdateCameraRotation(targetPosition);
    }
    
    private void UpdateCameraRotation(Vector3 targetPosition)
    {
        // Camera nhìn về phía player
        Vector3 lookDirection = targetPosition - transform.position;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
        }
    }
}
