using System.Collections;
using UnityEngine;

public class RopeAction : MonoBehaviour
{
    private LineRenderer lr;
    private SpringJoint sj;
    private bool isRoped = false;
    
    // isShooting 대신 발사/회수 두 가지 애니메이션 상태를 모두 체크하기 위해 이름을 바꿨습니다.
    private bool isAnimating = false; 
    private Transform targetAnchor;
    private Coroutine ropeCoroutine;

    [Header("로프 설정")]
    public KeyCode ropeKey;      
    public float ropeRange = 7f; 
    public float ropeSpring = 50f; 
    public float ropeDamper = 5f;  
    public Transform ropeLaunchPoint; 

    [Header("로프 애니메이션 설정")]
    public float ropeShootSpeed = 40f; 

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        sj = GetComponent<SpringJoint>();

        lr.enabled = false;
        sj.spring = 0f;
        sj.damper = 0f;
        sj.autoConfigureConnectedAnchor = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(ropeKey))
        {
            // 로프가 연결되어 있거나, 무언가 연출(발사/회수)이 진행 중이라면 무조건 끊고 회수합니다.
            if (isRoped || isAnimating) Detach();
            else TryAttach();
        }

        // 뻗어나가거나 감기는 애니메이션 중이 아닐 때(완전히 연결된 상태)만 위치를 업데이트합니다.
        if (isRoped && targetAnchor != null && !isAnimating)
        {
            lr.SetPosition(0, ropeLaunchPoint.position);
            lr.SetPosition(1, targetAnchor.position);
        }
    }

    void TryAttach()
    {
        Collider[] cols = Physics.OverlapSphere(ropeLaunchPoint.position, ropeRange);
        foreach (var col in cols)
        {
            if (col.CompareTag("Anchor"))
            {
                targetAnchor = col.transform;
                
                if (ropeCoroutine != null) StopCoroutine(ropeCoroutine);
                // 발사 애니메이션 시작
                ropeCoroutine = StartCoroutine(AnimateRopeShoot());
                return;
            }
        }
    }

    // [로프가 뻗어 나가는 애니메이션]
    IEnumerator AnimateRopeShoot()
    {
        isAnimating = true;
        lr.enabled = true;
        lr.positionCount = 2;

        Vector3 currentEndPos = ropeLaunchPoint.position;

        while (Vector3.Distance(currentEndPos, targetAnchor.position) > 0.1f)
        {
            currentEndPos = Vector3.MoveTowards(currentEndPos, targetAnchor.position, ropeShootSpeed * Time.deltaTime);

            lr.SetPosition(0, ropeLaunchPoint.position);
            lr.SetPosition(1, currentEndPos);

            yield return null;
        }

        isAnimating = false;
        AttachPhysics(); // 연출이 끝나면 물리 엔진 연결
    }

    void AttachPhysics()
    {
        isRoped = true;

        sj.anchor = transform.InverseTransformPoint(ropeLaunchPoint.position); 
        sj.connectedAnchor = targetAnchor.position;

        sj.spring = ropeSpring;
        sj.damper = ropeDamper;
        sj.maxDistance = Vector3.Distance(ropeLaunchPoint.position, targetAnchor.position);
    }

    void Detach()
    {
        // 1. 물리적 연결은 즉시 해제하여 플레이어가 즉각적으로 움직일 수 있게 합니다.
        isRoped = false;
        sj.spring = 0f;
        sj.damper = 0f;
        targetAnchor = null;

        // 2. 진행 중이던 발사/회수 연출 코루틴을 강제로 정지합니다.
        if (ropeCoroutine != null) StopCoroutine(ropeCoroutine);

        // 3. 선이 켜져 있는 상태라면, 현재 선의 끝점에서부터 감아들이는 연출을 시작합니다.
        if (lr.enabled)
        {
            Vector3 currentEndPos = lr.GetPosition(1); // 현재 뻗어있는 선의 끝점 좌표 가져오기
            ropeCoroutine = StartCoroutine(AnimateRopeRetract(currentEndPos));
        }
        else
        {
            isAnimating = false;
        }
    }

    // [로프가 다시 감겨 돌아오는 애니메이션]
    IEnumerator AnimateRopeRetract(Vector3 startRetractPos)
    {
        isAnimating = true;
        Vector3 currentEndPos = startRetractPos; // 회수를 시작할 위치

        // 선의 끝점이 발사 지점(손)에 도달할 때까지 반복
        while (Vector3.Distance(currentEndPos, ropeLaunchPoint.position) > 0.1f)
        {
            // MoveTowards를 사용해 끝점을 우주인 쪽으로 일정한 속도로 이동
            currentEndPos = Vector3.MoveTowards(currentEndPos, ropeLaunchPoint.position, ropeShootSpeed * Time.deltaTime);

            lr.SetPosition(0, ropeLaunchPoint.position);
            lr.SetPosition(1, currentEndPos);

            yield return null;
        }

        // 완전히 감겼으므로 선을 끄고 애니메이션 상태 종료
        lr.enabled = false;
        isAnimating = false;
    }
}