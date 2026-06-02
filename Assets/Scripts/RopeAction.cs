using System.Collections;
using UnityEngine;

public class RopeAction : MonoBehaviour
{
    public static int connectedRopes = 0;
    
    // [지형(Anchor)용 컴포넌트]
    private LineRenderer lrAnchor;
    private SpringJoint sjAnchor;
    private bool isAnchorRoped = false;
    private bool isAnchorAnimating = false;
    private Transform targetAnchor;
    private Coroutine anchorCoroutine;
    
    // [플레이어(Player)용 컴포넌트]
    private LineRenderer lrPlayer;
    private SpringJoint sjPlayer;
    private bool isPlayerRoped = false;
    private bool isPlayerAnimating = false;
    private Transform targetPlayer;
    private Coroutine playerCoroutine;
    private GameObject playerHookObject; 

    private Animator anim; 

    [Header("로프 비주얼 설정")]
    public GameObject hookObject; 

    [Header("로프 사운드 설정")]
    public AudioSource ropeSfxSource;
    public AudioClip ropeConnectSound;

    [Header("로프 설정")]
    public KeyCode ropeKey1;
    public KeyCode ropeKey2;      
    
    [Tooltip("로프가 닿는 최대 거리입니다.")]
    public float ropeRange = 7f; 
    
    public float ropeSpring = 50f; 
    public float ropeDamper = 5f;  
    
    public Transform ropeLaunchPoint; 

    [Header("로프 애니메이션 설정")]
    public float ropeShootSpeed = 40f; 

    [Header("조작 설정")]
    [Tooltip("플레이어 연결/해제에 필요한 꾹 누르는 시간")]
    public float chargeTimeRequired = 0.5f;

    [HideInInspector] public bool isHoldingKey = false;
    [HideInInspector] public float holdTimer = 0f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>(); 
    }
    
    void Awake()
    {
        connectedRopes = 0;
        
        lrAnchor = GetComponent<LineRenderer>();
        sjAnchor = GetComponent<SpringJoint>();
        lrAnchor.enabled = false;
        sjAnchor.spring = 0f;
        sjAnchor.damper = 0f;
        sjAnchor.autoConfigureConnectedAnchor = false;

        GameObject playerRopeObj = new GameObject("PlayerRopeRenderer");
        playerRopeObj.transform.SetParent(transform);
        playerRopeObj.transform.localPosition = Vector3.zero;

        lrPlayer = playerRopeObj.AddComponent<LineRenderer>();
        lrPlayer.sharedMaterial = lrAnchor.sharedMaterial;
        lrPlayer.startWidth = lrAnchor.startWidth;
        lrPlayer.endWidth = lrAnchor.endWidth;
        lrPlayer.startColor = lrAnchor.startColor;
        lrPlayer.endColor = lrAnchor.endColor;
        lrPlayer.textureMode = lrAnchor.textureMode;
        lrPlayer.positionCount = 2;
        lrPlayer.enabled = false;

        sjPlayer = gameObject.AddComponent<SpringJoint>();
        sjPlayer.autoConfigureConnectedAnchor = false;
        sjPlayer.spring = 0f;
        sjPlayer.damper = 0f;

        if (hookObject != null)
        {
            hookObject.SetActive(false);
            playerHookObject = Instantiate(hookObject, transform);
            playerHookObject.name = "PlayerHookObject";
            playerHookObject.SetActive(false);
        }
    }

    void Update()
    {
        if (IsRopeKeyDown())
        {
            isHoldingKey = true;
            holdTimer = 0f;
        }

        if (IsRopeKey() && isHoldingKey)
        {
            holdTimer += Time.deltaTime;
            
            if (holdTimer >= chargeTimeRequired)
            {
                isHoldingKey = false;
                
                RopeAction otherRope = GetOtherRope();
                
                if (HasPlayerRopeActive())
                {
                    DetachPlayer();
                }
                else if (otherRope != null && otherRope.HasPlayerRopeActive())
                {
                    otherRope.DetachPlayer();
                }
                else
                {
                    if (otherRope != null)
                    {
                        float distanceToPartner = Vector3.Distance(ropeLaunchPoint.position, otherRope.ropeLaunchPoint.position);
                        
                        if (distanceToPartner <= ropeRange)
                        {
                            TargetOtherPlayer(otherRope);
                        }
                    }
                }
            }
        }

        if (IsRopeKeyUp() && isHoldingKey)
        {
            isHoldingKey = false; 

            if (isAnchorAnimating || isAnchorRoped)
            {
                DetachAnchor(); 
            }
            else
            {
                TryAttachToAnchor(); 
            }
        }

        if (isAnchorRoped && targetAnchor != null && !isAnchorAnimating)
        {
            lrAnchor.SetPosition(0, ropeLaunchPoint.position);
            lrAnchor.SetPosition(1, targetAnchor.position);
            if (hookObject != null)
            {
                hookObject.transform.position = targetAnchor.position;
                Vector3 dir = ropeLaunchPoint.position - targetAnchor.position;
                if (dir != Vector3.zero) hookObject.transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        if (isPlayerRoped && targetPlayer != null && !isPlayerAnimating)
        {
            lrPlayer.SetPosition(0, ropeLaunchPoint.position);
            lrPlayer.SetPosition(1, targetPlayer.position);
            if (playerHookObject != null)
            {
                playerHookObject.transform.position = targetPlayer.position;
                Vector3 dir = ropeLaunchPoint.position - targetPlayer.position;
                if (dir != Vector3.zero) playerHookObject.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    void UpdateAnimState()
    {
        bool tying = isAnchorRoped || isPlayerRoped;
        if (anim != null) anim.SetBool("isTying", tying);
    }

    void TryAttachToAnchor()
    {
        Collider[] cols = Physics.OverlapSphere(ropeLaunchPoint.position, ropeRange);
        Transform closestTarget = null;
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            if (col.gameObject == this.gameObject) continue;
            if (col.CompareTag("Anchor"))
            {
                float dist = Vector3.Distance(ropeLaunchPoint.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestTarget = col.transform;
                }
            }
        }

        if (closestTarget != null)
        {
            targetAnchor = closestTarget;
            if (anchorCoroutine != null) StopCoroutine(anchorCoroutine);
            anchorCoroutine = StartCoroutine(AnimateAnchorShoot());
        }
    }

    IEnumerator AnimateAnchorShoot()
    {
        isAnchorAnimating = true;
        lrAnchor.enabled = true;
        lrAnchor.positionCount = 2;
        if (hookObject != null) { hookObject.SetActive(true); hookObject.transform.position = ropeLaunchPoint.position; hookObject.transform.SetParent(null); }

        Vector3 curPos = ropeLaunchPoint.position;
        while (Vector3.Distance(curPos, targetAnchor.position) > 0.1f)
        {
            curPos = Vector3.MoveTowards(curPos, targetAnchor.position, ropeShootSpeed * Time.deltaTime);
            lrAnchor.SetPosition(0, ropeLaunchPoint.position);
            lrAnchor.SetPosition(1, curPos);
            if (hookObject != null)
            {
                hookObject.transform.position = curPos;
                Vector3 dir = targetAnchor.position - ropeLaunchPoint.position;
                if (dir != Vector3.zero) hookObject.transform.rotation = Quaternion.LookRotation(dir);
            }
            yield return null;
        }
        isAnchorAnimating = false;
        AttachAnchorPhysics();
    }

    void AttachAnchorPhysics()
    {
        if (!isAnchorRoped && !isPlayerRoped) connectedRopes++; 
        
        isAnchorRoped = true;
        UpdateAnimState();
        if (ropeSfxSource != null && ropeConnectSound != null) ropeSfxSource.PlayOneShot(ropeConnectSound);

        sjAnchor.anchor = transform.InverseTransformPoint(ropeLaunchPoint.position); 
        sjAnchor.enableCollision = true;

        if (hookObject != null) { hookObject.transform.position = targetAnchor.position; hookObject.transform.SetParent(targetAnchor); }

        Rigidbody targetRb = targetAnchor.GetComponentInParent<Rigidbody>();
        if (targetRb != null) { sjAnchor.connectedBody = targetRb; sjAnchor.connectedAnchor = targetRb.transform.InverseTransformPoint(targetAnchor.position); }
        else { sjAnchor.connectedBody = null; sjAnchor.connectedAnchor = targetAnchor.position; }

        sjAnchor.spring = ropeSpring;
        sjAnchor.damper = ropeDamper;
        
        float actualDistance = Vector3.Distance(ropeLaunchPoint.position, targetAnchor.position);
        sjAnchor.maxDistance = Mathf.Min(actualDistance, ropeRange);
    }

    void DetachAnchor()
    {
        if (isAnchorRoped && !isPlayerRoped) connectedRopes--;
        isAnchorRoped = false;
        UpdateAnimState();

        sjAnchor.spring = 0f;
        sjAnchor.damper = 0f;
        sjAnchor.connectedBody = null; 
        targetAnchor = null;

        if (hookObject != null) hookObject.transform.SetParent(null);
        if (anchorCoroutine != null) StopCoroutine(anchorCoroutine);

        if (lrAnchor.enabled) anchorCoroutine = StartCoroutine(AnimateAnchorRetract(lrAnchor.GetPosition(1)));
        else isAnchorAnimating = false;
    }

    IEnumerator AnimateAnchorRetract(Vector3 startPos)
    {
        isAnchorAnimating = true;
        Vector3 curPos = startPos; 
        while (Vector3.Distance(curPos, ropeLaunchPoint.position) > 0.1f)
        {
            curPos = Vector3.MoveTowards(curPos, ropeLaunchPoint.position, ropeShootSpeed * Time.deltaTime);
            lrAnchor.SetPosition(0, ropeLaunchPoint.position);
            lrAnchor.SetPosition(1, curPos);
            if (hookObject != null)
            {
                hookObject.transform.position = curPos;
                Vector3 dir = ropeLaunchPoint.position - curPos;
                if (dir != Vector3.zero) hookObject.transform.rotation = Quaternion.LookRotation(dir);
            }
            yield return null;
        }
        if (hookObject != null) { hookObject.transform.SetParent(ropeLaunchPoint); hookObject.transform.localPosition = Vector3.zero; hookObject.SetActive(false); }
        lrAnchor.enabled = false;
        isAnchorAnimating = false;
    }

    public bool HasPlayerRopeActive()
    {
        return isPlayerRoped || isPlayerAnimating;
    }

    void TargetOtherPlayer(RopeAction otherRope)
    {
        if (otherRope != null)
        {
            targetPlayer = otherRope.ropeLaunchPoint;
            if (playerCoroutine != null) StopCoroutine(playerCoroutine);
            playerCoroutine = StartCoroutine(AnimatePlayerShoot());
        }
    }

    IEnumerator AnimatePlayerShoot()
    {
        isPlayerAnimating = true;
        lrPlayer.enabled = true;
        lrPlayer.positionCount = 2;
        if (playerHookObject != null) { playerHookObject.SetActive(true); playerHookObject.transform.position = ropeLaunchPoint.position; playerHookObject.transform.SetParent(null); }

        Vector3 curPos = ropeLaunchPoint.position;
        while (Vector3.Distance(curPos, targetPlayer.position) > 0.1f)
        {
            curPos = Vector3.MoveTowards(curPos, targetPlayer.position, ropeShootSpeed * Time.deltaTime);
            lrPlayer.SetPosition(0, ropeLaunchPoint.position);
            lrPlayer.SetPosition(1, curPos);
            if (playerHookObject != null)
            {
                playerHookObject.transform.position = curPos;
                Vector3 dir = targetPlayer.position - ropeLaunchPoint.position;
                if (dir != Vector3.zero) playerHookObject.transform.rotation = Quaternion.LookRotation(dir);
            }
            yield return null;
        }
        isPlayerAnimating = false;
        AttachPlayerPhysics();
    }

    void AttachPlayerPhysics()
    {
        isPlayerRoped = true;
        UpdateAnimState();
        if (ropeSfxSource != null && ropeConnectSound != null) ropeSfxSource.PlayOneShot(ropeConnectSound);

        sjPlayer.anchor = transform.InverseTransformPoint(ropeLaunchPoint.position); 
        sjPlayer.enableCollision = true;

        if (playerHookObject != null) { playerHookObject.transform.position = targetPlayer.position; playerHookObject.transform.SetParent(targetPlayer); }

        Rigidbody targetRb = targetPlayer.GetComponentInParent<Rigidbody>();
        if (targetRb != null) { sjPlayer.connectedBody = targetRb; sjPlayer.connectedAnchor = targetRb.transform.InverseTransformPoint(targetPlayer.position); }
        else { sjPlayer.connectedBody = null; sjPlayer.connectedAnchor = targetPlayer.position; }

        sjPlayer.spring = ropeSpring;
        sjPlayer.damper = ropeDamper;
        
        float actualDistance = Vector3.Distance(ropeLaunchPoint.position, targetPlayer.position);
        sjPlayer.maxDistance = Mathf.Min(actualDistance, ropeRange);
    }

    public void DetachPlayer()
    {
        isPlayerRoped = false;
        UpdateAnimState();

        sjPlayer.spring = 0f;
        sjPlayer.damper = 0f;
        sjPlayer.connectedBody = null; 
        targetPlayer = null;

        if (playerHookObject != null) playerHookObject.transform.SetParent(null);
        if (playerCoroutine != null) StopCoroutine(playerCoroutine);

        if (lrPlayer.enabled) playerCoroutine = StartCoroutine(AnimatePlayerRetract(lrPlayer.GetPosition(1)));
        else isPlayerAnimating = false;
    }

    IEnumerator AnimatePlayerRetract(Vector3 startPos)
    {
        isPlayerAnimating = true;
        Vector3 curPos = startPos; 
        while (Vector3.Distance(curPos, ropeLaunchPoint.position) > 0.1f)
        {
            curPos = Vector3.MoveTowards(curPos, ropeLaunchPoint.position, ropeShootSpeed * Time.deltaTime);
            lrPlayer.SetPosition(0, ropeLaunchPoint.position);
            lrPlayer.SetPosition(1, curPos);
            if (playerHookObject != null)
            {
                playerHookObject.transform.position = curPos;
                Vector3 dir = ropeLaunchPoint.position - curPos;
                if (dir != Vector3.zero) playerHookObject.transform.rotation = Quaternion.LookRotation(dir);
            }
            yield return null;
        }
        if (playerHookObject != null) { playerHookObject.transform.SetParent(ropeLaunchPoint); playerHookObject.transform.localPosition = Vector3.zero; playerHookObject.SetActive(false); }
        lrPlayer.enabled = false;
        isPlayerAnimating = false;
    }

    RopeAction GetOtherRope()
    {
        RopeAction[] ropes = FindObjectsOfType<RopeAction>();
        foreach (var r in ropes) if (r != this) return r;
        return null;
    }

    public RopeAction GetWhoRopedMe()
    {
        RopeAction[] ropes = FindObjectsOfType<RopeAction>();
        foreach (var r in ropes)
        {
            if (r != this && r.HasPlayerRopeActive() && r.targetPlayer == this.ropeLaunchPoint)
            {
                return r;
            }
        }
        return null;
    }

    bool IsRopeKeyDown()
    {
        return (ropeKey1 != KeyCode.None && Input.GetKeyDown(ropeKey1)) || (ropeKey2 != KeyCode.None && Input.GetKeyDown(ropeKey2));
    }
    bool IsRopeKey()
    {
        return (ropeKey1 != KeyCode.None && Input.GetKey(ropeKey1)) || (ropeKey2 != KeyCode.None && Input.GetKey(ropeKey2));
    }
    bool IsRopeKeyUp()
    {
        return (ropeKey1 != KeyCode.None && Input.GetKeyUp(ropeKey1)) || (ropeKey2 != KeyCode.None && Input.GetKeyUp(ropeKey2));
    }

    public void CutAllRopes()
    {
        if (HasPlayerRopeActive()) DetachPlayer();
        
        if (isAnchorRoped || isAnchorAnimating) DetachAnchor();
        
        RopeAction whoRopedMe = GetWhoRopedMe();
        if (whoRopedMe != null) whoRopedMe.DetachPlayer();
    }
}