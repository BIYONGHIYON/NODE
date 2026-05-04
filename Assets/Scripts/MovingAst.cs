using UnityEngine;

public class MovingAst : MonoBehaviour
{
    public float thrust = 15f; 
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f; 
    public float idleTimeBeforeReset = 1f; 

    private Rigidbody rb;
    private Animator anim; 

    [Header("Player Controls")]
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;

    private float currentIdleTime = 0f; 
    
    // Update에서 입력받은 값을 FixedUpdate로 전달하기 위한 변수
    private float inputMoveX = 0f;
    private float inputMoveY = 0f;
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>(); 
        
        if (anim == null)
        {
            Debug.LogError("애니메이터를 찾을 수 없습니다! 구조를 확인해 주세요.");
        }
    }

    void Update()
    {
        // 1. 매 프레임 입력값 갱신 (누락 방지)
        inputMoveX = 0f;
        inputMoveY = 0f;

        if (Input.GetKey(upKey)) inputMoveY += 0.4f;
        if (Input.GetKey(downKey)) inputMoveY -= 0.8f;
        if (Input.GetKey(rightKey)) inputMoveX += 1f;
        if (Input.GetKey(leftKey)) inputMoveX -= 1f;

        Vector3 moveDirection = new Vector3(inputMoveX, inputMoveY, 0).normalized;
        isMoving = moveDirection != Vector3.zero;

        // 2. 애니메이션 상태 업데이트
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                anim.SetBool("isTying", false);
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(inputMoveX, inputMoveY, 0).normalized;

        if (isMoving)
        {
            currentIdleTime = 0f;

            // 1. 이동 (힘 가하기)
            rb.AddForce(moveDirection * thrust);

            // 2. 회전 로직
            Vector3 lookDirection = moveDirection;

            if (inputMoveX == 0f && inputMoveY != 0f)
            {
                lookDirection = new Vector3(0f, inputMoveY, -1f).normalized;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            currentIdleTime += Time.fixedDeltaTime;

            if (currentIdleTime >= idleTimeBeforeReset)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.back);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
                if (anim != null) anim.SetBool("isTying", false);
            }
        }
    }
}