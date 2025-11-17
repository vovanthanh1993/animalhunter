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

    private void LateUpdate()
    {
        if (target == null) return;

        // Quay camera theo góc cố định
        Quaternion rotation = Quaternion.Euler(angleX, angleY, 0f);

        // Vị trí mong muốn của camera (ở sau/lệch so với target)
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
        Vector3 desiredPosition = target.position + offset + Vector3.up * heightOffset;

        // Di chuyển camera mượt mà đến vị trí mong muốn
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Nhìn về phía target
        transform.LookAt(target.position + Vector3.up * heightOffset);
    }
}
