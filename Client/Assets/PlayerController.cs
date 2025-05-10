using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Title("스테이터스")]
    [LabelText("플레이어 현재 체력")]
    public int currentPlayerHP;

    [LabelText("플레이어 최대 체력")] public int playerMaxHP;
    
    [Title("이동 관련")]
    [LabelText("이동 속도")]
    public float runSpeed     = 4.0f;
    [LabelText("방향 전환 속도")]
    public float turnSpeed    = 12f;
    [LabelText("가속도")]
    public float acceleration = 8.0f;
    [LabelText("감속도")]
    public float deceleration = 20f;

    [SerializeField, LabelText("카메라")]
    private Transform cam;
    
    [SerializeField, LabelText("애니메이터")]
    private Animator animator;
    
    CharacterController cc;
    float currentSpeed;
    bool attackQueued;
    
    readonly bool[] skillQueued = new bool[4];

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        ReadInput();
        Move();
        Animate();
    }

    void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.K)) attackQueued = true;

        if (Input.GetKeyDown(KeyCode.Keypad1)) skillQueued[0] = true;
        if (Input.GetKeyDown(KeyCode.Keypad2)) skillQueued[1] = true;
        if (Input.GetKeyDown(KeyCode.Keypad3)) skillQueued[2] = true;
        if (Input.GetKeyDown(KeyCode.Keypad4)) skillQueued[3] = true;
    }

    void Move()
    {
        Vector3 camF = cam.forward;  camF.y = 0;  camF.Normalize();
        Vector3 camR = cam.right;    camR.y = 0;  camR.Normalize();
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");  
        
        Vector3 dir = (camF * v + camR * h).normalized;
        
        float accel = dir.sqrMagnitude < 0.001f ? deceleration : acceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed,
            dir.magnitude * runSpeed,
            accel * Time.deltaTime);
        
        cc.Move(dir * currentSpeed * Time.deltaTime);
        
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                targetRot,
                turnSpeed * Time.deltaTime);
        }
    }

    void Animate()
    {
        float speed01 = currentSpeed > 0.1f ? 1f : 0f;
        animator.SetFloat("MoveSpeed", speed01, 0.05f, Time.deltaTime);

        if (attackQueued)
        {
            animator.SetTrigger("Attack");
            attackQueued = false;
        }
    }
}
