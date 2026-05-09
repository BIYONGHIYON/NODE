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

    [Header("SFX Settings")]
    public AudioSource moveSfxSource;
    public float resumeTimeWindow = 0.5f; 
    
    // ==========================================
    // [추가됨] 페이드 아웃 속도를 조절하는 변수
    public float audioFadeSpeed = 10f; 
    private float originalVolume = 1f; // 에디터에서 설정한 원래 볼륨값을 저장할 변수
    // ==========================================

    private float lastStopTime = -100f; 

    private float currentIdleTime = 0f; 
    
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

        if (moveSfxSource != null)
        {
            originalVolume = moveSfxSource.volume;
            moveSfxSource.volume = 0f;
        }
    }

    void Update()
    {
        inputMoveX = 0f;
        inputMoveY = 0f;

        if (Input.GetKey(upKey)) inputMoveY += 0.4f;
        if (Input.GetKey(downKey)) inputMoveY -= 0.8f;
        if (Input.GetKey(rightKey)) inputMoveX += 1f;
        if (Input.GetKey(leftKey)) inputMoveX -= 1f;

        Vector3 moveDirection = new Vector3(inputMoveX, inputMoveY, 0).normalized;
        isMoving = moveDirection != Vector3.zero;

        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                anim.SetBool("isTying", false);
            }
        }

        if (moveSfxSource != null)
        {
            if (isMoving)
            {
                if (!moveSfxSource.isPlaying)
                {
                    if (Time.time - lastStopTime > resumeTimeWindow)
                    {
                        moveSfxSource.time = 0f;
                    }
                    moveSfxSource.Play();
                }
                
                moveSfxSource.volume = Mathf.Lerp(moveSfxSource.volume, originalVolume, Time.deltaTime * audioFadeSpeed);
            }
            else
            {
                if (moveSfxSource.isPlaying)
                {
                    moveSfxSource.volume = Mathf.Lerp(moveSfxSource.volume, 0f, Time.deltaTime * audioFadeSpeed);
                    
                    if (moveSfxSource.volume <= 0.05f)
                    {
                        moveSfxSource.volume = 0f;
                        moveSfxSource.Pause();
                        lastStopTime = Time.time;
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(inputMoveX, inputMoveY, 0).normalized;

        if (isMoving)
        {
            currentIdleTime = 0f;

            rb.AddForce(moveDirection * thrust);

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