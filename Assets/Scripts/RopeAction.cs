using System.Collections;
using UnityEngine;

public class RopeAction : MonoBehaviour
{
    public static int connectedRopes = 0;
    private LineRenderer lr;
    private SpringJoint sj;
    private bool isRoped = false;
    
    private bool isTying = false; 
    private bool isAnimating = false; 
    private Transform targetAnchor;
    private Coroutine ropeCoroutine;
    private Animator anim; 

    // =========================================================================
    [Header("로프 비주얼 설정")]
    public GameObject hookObject; 

    [Header("로프 사운드 설정")]
    public AudioSource ropeSfxSource;
    public AudioClip ropeConnectSound;
    // =========================================================================

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
        connectedRopes = 0;
        lr = GetComponent<LineRenderer>();
        sj = GetComponent<SpringJoint>();

        lr.enabled = false;
        sj.spring = 0f;
        sj.damper = 0f;
        sj.autoConfigureConnectedAnchor = false;

        if (hookObject != null)
        {
            hookObject.SetActive(false);
        }
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

            if (hookObject != null)
            {
                hookObject.transform.position = targetAnchor.position;
                Vector3 direction = ropeLaunchPoint.position - targetAnchor.position;
                if (direction != Vector3.zero)
                {
                    hookObject.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }

    void TryAttach()
    {
        Collider[] cols = Physics.OverlapSphere(ropeLaunchPoint.position, ropeRange);
        
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var col in cols)
        {
            if (col.gameObject == this.gameObject) continue;

            if (col.CompareTag("Anchor") || col.CompareTag("Player"))
            {
                Transform potentialTarget = col.transform;

                if (col.CompareTag("Player"))
                {
                    RopeAction targetRope = col.GetComponent<RopeAction>();
                    if (targetRope != null && targetRope.ropeLaunchPoint != null)
                    {
                        potentialTarget = targetRope.ropeLaunchPoint;
                    }
                }

                float dist = Vector3.Distance(ropeLaunchPoint.position, potentialTarget.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTarget = potentialTarget;
                }
            }
        }

        if (closestTarget != null)
        {
            targetAnchor = closestTarget;
            
            if (ropeCoroutine != null) StopCoroutine(ropeCoroutine);
            ropeCoroutine = StartCoroutine(AnimateRopeShoot());
        }
    }

    IEnumerator AnimateRopeShoot()
    {
        isAnimating = true;
        lr.enabled = true;
        lr.positionCount = 2;

        if (hookObject != null)
        {
            hookObject.SetActive(true);
            hookObject.transform.position = ropeLaunchPoint.position;
            hookObject.transform.SetParent(null); 
        }

        Vector3 currentEndPos = ropeLaunchPoint.position;

        while (Vector3.Distance(currentEndPos, targetAnchor.position) > 0.1f)
        {
            currentEndPos = Vector3.MoveTowards(currentEndPos, targetAnchor.position, ropeShootSpeed * Time.deltaTime);

            lr.SetPosition(0, ropeLaunchPoint.position);
            lr.SetPosition(1, currentEndPos);

            if (hookObject != null)
            {
                hookObject.transform.position = currentEndPos;
                Vector3 direction = targetAnchor.position - ropeLaunchPoint.position;
                if (direction != Vector3.zero)
                {
                    hookObject.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            yield return null;
        }

        isAnimating = false;
        AttachPhysics(); 
    }

    void AttachPhysics()
    {
        if (!isRoped)
        {
            connectedRopes++;
            if (connectedRopes >= 2)
            {
                TutorialTextController tutorialText = FindObjectOfType<TutorialTextController>();
                if (tutorialText != null)
                {
                    tutorialText.OnBothHooksConnected();
                }
            }
        }
        isRoped = true;
        isTying = true;
        if (anim != null) anim.SetBool("isTying", isTying);

        if (ropeSfxSource != null && ropeConnectSound != null)
        {
            ropeSfxSource.PlayOneShot(ropeConnectSound);
        }

        sj.anchor = transform.InverseTransformPoint(ropeLaunchPoint.position); 

        if (hookObject != null)
        {
            hookObject.transform.position = targetAnchor.position; 
            hookObject.transform.SetParent(targetAnchor);
        }

        Rigidbody targetRb = targetAnchor.GetComponentInParent<Rigidbody>();

        if (targetRb != null && targetRb.CompareTag("Player"))
        {
            sj.connectedBody = targetRb;
            sj.connectedAnchor = targetRb.transform.InverseTransformPoint(targetAnchor.position);
        }
        else
        {
            sj.connectedBody = null; 
            sj.connectedAnchor = targetAnchor.position;
        }

        sj.spring = ropeSpring;
        sj.damper = ropeDamper;
        sj.maxDistance = Vector3.Distance(ropeLaunchPoint.position, targetAnchor.position);
    }

    void Detach()
    {
        if (isRoped)
        {
            connectedRopes--;
        }
        isRoped = false;
        isTying = false; 
        if (anim != null) anim.SetBool("isTying", isTying);

        sj.spring = 0f;
        sj.damper = 0f;
        sj.connectedBody = null; 
        targetAnchor = null;

        if (hookObject != null)
        {
            hookObject.transform.SetParent(null);
        }

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

            if (hookObject != null)
            {
                hookObject.transform.position = currentEndPos;
                Vector3 direction = ropeLaunchPoint.position - currentEndPos;
                if (direction != Vector3.zero)
                {
                    hookObject.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            yield return null;
        }

        if (hookObject != null)
        {
            hookObject.transform.SetParent(ropeLaunchPoint);
            hookObject.transform.localPosition = Vector3.zero;
            hookObject.transform.localRotation = Quaternion.identity;
            
            hookObject.SetActive(false);
        }

        lr.enabled = false;
        isAnimating = false;
    }
}