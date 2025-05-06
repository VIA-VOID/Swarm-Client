using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed   = 2.0f;
    public float runSpeed    = 4.0f;
    public float acceleration = 8.0f;

    [Header("Gravity / Jump")]
    public float gravity     = -9.81f;
    public float jumpPower   = 4f;

    CharacterController cc;
    Animator anim;
    Vector3 velocity;
    float currentSpeed;

    void Awake()
    {
        cc   = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        Move();
        ApplyGravity();
        Animate();
    }

    #region ── Movement ────────────────────────────────────────────
    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");   // A/D 또는 ← →
        float v = Input.GetAxisRaw("Vertical");     // W/S 또는 ↑ ↓
        Vector3 dir = new Vector3(h, 0, v).normalized;

        // 달리기 토글(Shift) ‧ 모바일은 버튼으로 치환 가능
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, dir.magnitude * targetSpeed, acceleration * Time.deltaTime);

        // 로컬 기준 → 월드 변환 (캐릭터가 바라보는 방향)
        Vector3 move = transform.TransformDirection(dir) * currentSpeed;
        cc.Move(move * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (cc.isGrounded && velocity.y < 0)
            velocity.y = -2f;     // 살짝 눌러 붙도록

        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded)
            velocity.y = jumpPower;

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region ── Animation ───────────────────────────────────────────
    void Animate()
    {
        float animSpeed = currentSpeed / runSpeed;      // 0 ~ 1
        anim.SetFloat("MoveSpeed", animSpeed, 0.1f, Time.deltaTime);  // DampTime=0.1

        anim.SetBool ("IsGround", cc.isGrounded);
    }
    #endregion
}
