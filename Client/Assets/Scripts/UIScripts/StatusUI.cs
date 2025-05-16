using UnityEngine;

public class StatusUI : MonoBehaviour
{
    [Tooltip("지정하지 않으면 Camera.main 사용")]
    public Camera targetCamera;

    [Tooltip("Y축만 회전하고 싶으면 체크 (수평 뒤집기)")]
    public bool yawOnly = false;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (!targetCamera) return;

        if (yawOnly)
        {
            // 카메라 방향 벡터에서 Y만 회전
            Vector3 camPos = targetCamera.transform.position;
            Vector3 lookDir = new Vector3(camPos.x, transform.position.y, camPos.z) - transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(-lookDir);
        }
        else
        {
            // 정면으로 완전히 바라봄
            transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
        }
    }
}
