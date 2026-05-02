using UnityEngine;

public class MovingAst : MonoBehaviour
{
    public float thrust = 15f; // 추진력
    private Rigidbody rb;

    [Header("Player Controls")]
    // 인스펙터 창에서 플레이어별 조작키를 설정할 수 있도록 변수 선언
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveX = 0f;
        float moveY = 0f;

        // 개별 키 입력 확인
        if (Input.GetKey(upKey)) moveY += 1f;
        if (Input.GetKey(downKey)) moveY -= 1f;
        if (Input.GetKey(rightKey)) moveX += 1f;
        if (Input.GetKey(leftKey)) moveX -= 1f;

        // 대각선 이동 시 속도가 빨라지는 것을 막기 위해 정규화 (normalized)
        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;

        // 입력이 있을 때만 힘 가하기
        if (moveDirection != Vector3.zero)
        {
            rb.AddForce(moveDirection * thrust);
        }
    }
}