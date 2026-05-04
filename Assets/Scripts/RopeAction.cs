using System.Collections;
using UnityEngine;

public class RopeAction : MonoBehaviour
{
    private LineRenderer lr;
    private SpringJoint sj;
    private bool isRoped = false;
    
    // [추가됨] 로프 연결 상태를 체크하고 애니메이터에 전달하기 위한 변수
    private bool isTying = false; 

    private bool isAnimating = false; 
    private Transform targetAnchor;
    private Coroutine ropeCoroutine;
    private Animator anim; 

    [Header("로프 설정")]
    public KeyCode ropeKey1;
    public KeyCode ropeKey2;      
    public float ropeRange = 7f; 
    public float ropeSpring = 50f; 
    public float ropeDamper = 5f;  
    public Transform ropeLaunchPoint; 

    [Header("로프 애니메이션 설정")]
    public float ropeShootSpeed = 40f; 

    void Start()
    {
        anim = GetComponentInChildren<Animator>(); 
    }
    
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
        if (Input.GetKeyDown(ropeKey1) || Input.GetKeyDown(ropeKey2))
        {
            if (isRoped || isAnimating) Detach();
            else TryAttach();
        }

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
                ropeCoroutine = StartCoroutine(AnimateRopeShoot());
                return;
            }
        }
    }

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
        AttachPhysics(); 
    }

    void AttachPhysics()
    {
        isRoped = true;

        // [추가됨] 로프가 대상에 닿아 완전히 연결되는 순간 true
        isTying = true;
        if (anim != null) anim.SetBool("isTying", isTying);

        sj.anchor = transform.InverseTransformPoint(ropeLaunchPoint.position); 
        sj.connectedAnchor = targetAnchor.position;

        sj.spring = ropeSpring;
        sj.damper = ropeDamper;
        sj.maxDistance = Vector3.Distance(ropeLaunchPoint.position, targetAnchor.position);
    }

    void Detach()
    {
        isRoped = false;

        isTying = true;
        if (anim != null) anim.SetBool("isTying", isTying);

        sj.spring = 0f;
        sj.damper = 0f;
        targetAnchor = null;

        if (ropeCoroutine != null) StopCoroutine(ropeCoroutine);

        if (lr.enabled)
        {
            Vector3 currentEndPos = lr.GetPosition(1); 
            ropeCoroutine = StartCoroutine(AnimateRopeRetract(currentEndPos));
        }
        else
        {
            isAnimating = false;
        }
    }

    IEnumerator AnimateRopeRetract(Vector3 startRetractPos)
    {
        isAnimating = true;
        Vector3 currentEndPos = startRetractPos; 

        while (Vector3.Distance(currentEndPos, ropeLaunchPoint.position) > 0.1f)
        {
            currentEndPos = Vector3.MoveTowards(currentEndPos, ropeLaunchPoint.position, ropeShootSpeed * Time.deltaTime);

            lr.SetPosition(0, ropeLaunchPoint.position);
            lr.SetPosition(1, currentEndPos);

            yield return null;
        }

        lr.enabled = false;
        isAnimating = false;
    }
}