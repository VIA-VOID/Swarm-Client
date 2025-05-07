using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField, LabelText("추적 타겟")]
    private Transform target;
    
    [SerializeField, LabelText("줌 속도")]
    float zoomSpeed = 4f;
    [SerializeField, LabelText("최소 거리")]
    float minDistance = 2f;
    [SerializeField, LabelText("최대 거리")]
    float maxDistance = 10f;
    
    [Header("회전 값")]
    [SerializeField, LabelText("민감도")] float sensitivity = 2f;
    [SerializeField, LabelText("하한각")] float minPitch = -30f; 
    [SerializeField, LabelText("상한각")] float maxPitch = 60f;
    
    float distance;
    float yaw;                                 
    float pitch;

    private void Awake()
    {
        Vector3 vec = transform.position - target.position;
        distance = vec.magnitude;
        
        yaw   = transform.eulerAngles.y;
        
        float rawPitch = transform.eulerAngles.x;
        pitch = rawPitch > 180 ? rawPitch - 360 : rawPitch;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (!target) return;
        
        if (Input.GetMouseButton(1))
        {
            yaw   += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }
        
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rot * Vector3.back * distance;
        
        transform.position = target.position + offset;
        transform.rotation = rot;
    }
}
