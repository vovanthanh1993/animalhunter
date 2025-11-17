using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          // Nhân vật cần camera theo dõi

    [Header("Camera Settings")]
    public float distance = 10f;      // Khoảng cách từ camera tới nhân vật
    public float heightOffset = 0f;   // Độ cao cộng thêm so với target (tuỳ chọn)

    // Góc quay cố định: x = 45 độ (nghiêng từ trên xuống), y = 45 độ (xoay ngang)
    public float angleX = 45f;
    public float angleY = 45f;

    [Header("Follow Smoothing")]
    public float followSpeed = 10f;   // Tốc độ camera đuổi theo target

    private Vector3 lastTargetPosition;
    private const float positionThreshold = 0.001f; // Ngưỡng để xác định player có di chuyển hay không

    private void Start()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Kiểm tra xem player có thực sự di chuyển hay không
        Vector3 currentTargetPosition = target.position;
        float positionDelta = Vector3.Distance(currentTargetPosition, lastTargetPosition);
        bool isPlayerMoving = positionDelta > positionThreshold;

        // Chỉ cập nhật camera khi player thực sự di chuyển
        if (isPlayerMoving)
        {
            // Quay camera theo góc cố định
            Quaternion rotation = Quaternion.Euler(angleX, angleY, 0f);

            // Vị trí mong muốn của camera (ở sau/lệch so với target)
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
            Vector3 desiredPosition = currentTargetPosition + offset + Vector3.up * heightOffset;

            // Di chuyển camera mượt mà đến vị trí mong muốn
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            // Nhìn về phía target
            transform.LookAt(currentTargetPosition + Vector3.up * heightOffset);

            // Cập nhật vị trí cuối cùng của target
            lastTargetPosition = currentTargetPosition;
        }
        else
        {
            // Khi player không di chuyển, camera vẫn nhìn về target nhưng không di chuyển
            transform.LookAt(currentTargetPosition + Vector3.up * heightOffset);
        }
    }
}
