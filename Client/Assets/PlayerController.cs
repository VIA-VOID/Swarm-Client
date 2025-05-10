using System;
using System.Collections.Generic;
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

    [SerializeField, LabelText("애니메이션 리스너")]
    private AnimationListener animationListener;
    
    CharacterController cc;
    float currentSpeed;
    bool attackQueued;

    Dictionary<KeyCode, string> skillKeys = new Dictionary<KeyCode, string>
    {
        { KeyCode.Keypad1, "Skill1" },
        { KeyCode.Keypad2, "Skill2" },
        { KeyCode.Keypad3, "Skill3" },
        { KeyCode.Keypad4, "Skill4" },
        { KeyCode.Keypad5, "Skill5" }
    };
    
    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        animationListener.onEndStatus += AnimationListenerOnonEndStatus;
    }

    private void AnimationListenerOnonEndStatus()
    {
        attackQueued = false;
    }

    void Update()
    {
        ReadInput();
        Move();
        Animate();
    }

    void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            attackQueued = true;
            animator.SetTrigger("NormalAttack");
        }

        foreach (var skill in skillKeys)
        {
            if (Input.GetKeyDown(skill.Key))
            {
                Debug.Log(skill.Value);
                animator.SetTrigger(skill.Value);
                break;
            }
        }
    }

    void Move()
    {
        if (attackQueued) return;
        
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
    }
}
