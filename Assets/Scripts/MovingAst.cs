using UnityEngine;

public class MovingAst : MonoBehaviour
{
    public float thrust = 15f; // 추진력
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f; // 회전 속도
    public float idleTimeBeforeReset = 1f; // 정면을 보기까지의 대기 시간 (초)

    private Rigidbody rb;
    private Animator anim; 

    [Header("Player Controls")]
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;

    private float currentIdleTime = 0f; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>(); 
        
        if (anim == null)
        {
            Debug.LogError("애니메이터를 찾을 수 없습니다! 구조를 확인해 주세요.");
        }
    }

    void FixedUpdate()
    {
        float moveX = 0f;
        float moveY = 0f;

        // 개별 키 입력 확인
        if (Input.GetKey(upKey)) moveY += 0.4f;
        if (Input.GetKey(downKey)) moveY -= 0.8f;
        if (Input.GetKey(rightKey)) moveX += 1f;
        if (Input.GetKey(leftKey)) moveX -= 1f;

        // 실제 물리 이동에 쓰일 벡터 (이동 자체는 화면 평면인 XY축으로만 이루어짐)
        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;
        bool isMoving = moveDirection != Vector3.zero;

        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);

            // [추가됨] 키 입력으로 이동이 발생하면 isTying 상태를 강제로 false로 만듭니다.
            if (isMoving)
            {
                anim.SetBool("isTying", false);
            }
        }

        if (isMoving)
        {
            currentIdleTime = 0f;

            // 1. 이동: 방향 그대로 물리적인 힘 가하기
            rb.AddForce(moveDirection * thrust);

            // 2. 회전 벡터 계산
            Vector3 lookDirection = moveDirection;

            // 좌우 입력 없이 위/아래 입력만 있을 때 (순수 수직 이동)
            if (moveX == 0f && moveY != 0f)
            {
                lookDirection = new Vector3(0f, moveY, -1f).normalized;
            }

            // 계산된 방향으로 부드럽게 회전
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // 키 입력이 없으면 대기 시간 누적
            currentIdleTime += Time.fixedDeltaTime;

            // 지정된 시간이 지나면 완벽한 정면(화면 밖, 카메라 쪽)을 바라봄
            if (currentIdleTime >= idleTimeBeforeReset)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.back);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
                if (anim != null) anim.SetBool("isTying", false);
            }
        }
    }
}