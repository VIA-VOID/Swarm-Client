using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Title("이동 관련")]
    [LabelText("이동 속도")]
    public float runSpeed     = 4.0f;
    [LabelText("방향 전환 속도")]
    public float turnSpeed    = 12f;
    [LabelText("가속도")]
    public float acceleration = 8.0f;

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
        
        if (!cam) cam = Camera.main.transform;
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
        float h = Input.GetAxisRaw("Horizontal"); 
        float v = Input.GetAxisRaw("Vertical");   
        Vector3 dir = new Vector3(h, 0, v).normalized;
        
        currentSpeed = Mathf.MoveTowards(currentSpeed, dir.magnitude * runSpeed, acceleration * Time.deltaTime);

        Vector3 move = transform.TransformDirection(dir) * currentSpeed;
        cc.Move(move * Time.deltaTime);
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
