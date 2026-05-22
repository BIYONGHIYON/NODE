using System.Collections.Generic;
using UnityEngine;

public class GravityZone : MonoBehaviour
{
    [Header("중력 설정")]
    public Vector3 gravityDirection = Vector3.down;
    public float gravityStrength = 15f; 

    [Header("점프 설정")]
    public float jumpPower = 15f;
    public int maxJumps = 1; 

    [Header("플레이어 1 설정")]
    public string p1ObjectName = "Player1"; 
    public KeyCode p1JumpKey = KeyCode.W;

    [Header("플레이어 2 설정")]
    public string p2ObjectName = "Player2"; 
    public KeyCode p2JumpKey = KeyCode.UpArrow;

    private Dictionary<Rigidbody, int> playerJumpCounts = new Dictionary<Rigidbody, int>();
    private Dictionary<Rigidbody, bool> playerWasRoped = new Dictionary<Rigidbody, bool>();

    void Update()
    {
        List<Rigidbody> rbs = new List<Rigidbody>(playerJumpCounts.Keys);

        foreach (Rigidbody rb in rbs)
        {
            if (rb == null || rb.isKinematic) continue;

            // 1. 바닥에 닿아있으면 점프 횟수 리셋
            if (rb.velocity.y <= 0.1f && IsGrounded(rb))
            {
                playerJumpCounts[rb] = 0;
            }

            // ========================================================
            // 2. [수정됨] 나 또는 상대방의 로프 연결 상태를 모두 감지
            // ========================================================
            bool isCurrentlyRoped = false;
            
            // A. 내가 천장이나 상대방에게 로프를 직접 쏜 경우
            SpringJoint[] joints = rb.GetComponents<SpringJoint>();
            foreach (SpringJoint sj in joints)
            {
                if (sj.spring > 0f)
                {
                    isCurrentlyRoped = true;
                    break;
                }
            }

            // B. 내가 쏘지 않았지만, 상대방이 '나에게' 로프를 쏜 경우 (끌려가는 입장)
            if (!isCurrentlyRoped)
            {
                foreach (Rigidbody otherRb in rbs) 
                {
                    if (otherRb == rb) continue; // 나 자신은 제외
                    
                    SpringJoint[] otherJoints = otherRb.GetComponents<SpringJoint>();
                    foreach (SpringJoint osj in otherJoints)
                    {
                        // 상대방의 로프 힘이 들어간 상태인데, 그 끝이 나(rb)에게 연결되어 있다면?
                        if (osj.spring > 0f && osj.connectedBody == rb)
                        {
                            isCurrentlyRoped = true;
                            break;
                        }
                    }
                    if (isCurrentlyRoped) break;
                }
            }

            // 이전에는 로프에 연결되어 있었는데 지금 풀렸다면 점프 충전!
            if (!isCurrentlyRoped && playerWasRoped[rb])
            {
                playerJumpCounts[rb] = 0;
            }
            
            playerWasRoped[rb] = isCurrentlyRoped;
            // ========================================================

            bool shouldJump = false;

            // 3. 키 입력 확인
            if (rb.gameObject.name.Contains(p1ObjectName))
            {
                if (Input.GetKeyDown(p1JumpKey)) shouldJump = true;
            }
            else if (rb.gameObject.name.Contains(p2ObjectName))
            {
                if (Input.GetKeyDown(p2JumpKey)) shouldJump = true;
            }

            // 4. 점프 버튼을 눌렀고 남은 점프 횟수가 있다면 날아오릅니다!
            if (shouldJump && playerJumpCounts[rb] < maxJumps)
            {
                playerJumpCounts[rb]++; 
                
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
        }
    }

    bool IsGrounded(Rigidbody rb)
    {
        Collider col = rb.GetComponent<Collider>();
        if (col == null) return false;

        float distToGround = col.bounds.extents.y;
        
        RaycastHit[] hits = Physics.RaycastAll(col.bounds.center, Vector3.down, distToGround + 0.1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(rb.transform)) continue;
            if (hit.collider.CompareTag("Player")) continue;

            return true; 
        }
        
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            
            if (rb != null && !playerJumpCounts.ContainsKey(rb))
            {
                playerJumpCounts.Add(rb, 0);
                playerWasRoped.Add(rb, false); 
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(gravityDirection.normalized * gravityStrength, ForceMode.Acceleration);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            
            if (rb != null && playerJumpCounts.ContainsKey(rb))
            {
                playerJumpCounts.Remove(rb);
                playerWasRoped.Remove(rb); 
            }
        }
    }
}