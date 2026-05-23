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

    private Dictionary<Rigidbody, int> playerJumpCounts = new Dictionary<Rigidbody, int>();
    private Dictionary<Rigidbody, bool> playerWasRoped = new Dictionary<Rigidbody, bool>();
    
    // [핵심 추가] 각 플레이어가 마지막으로 점프한 시간을 기록하는 장부
    private Dictionary<Rigidbody, float> lastJumpTime = new Dictionary<Rigidbody, float>(); 

    void Update()
    {
        List<Rigidbody> rbs = new List<Rigidbody>(playerJumpCounts.Keys);

        foreach (Rigidbody rb in rbs)
        {
            if (rb == null || rb.isKinematic) continue;

            // 1. [버그 수정됨] 바닥에 닿아있어도, '마지막 점프 후 0.2초'가 지나야만 횟수를 리셋합니다!
            if (Time.time - lastJumpTime[rb] > 0.2f && rb.velocity.y <= 0.1f && IsGrounded(rb))
            {
                playerJumpCounts[rb] = 0;
            }

            // 2. 나 또는 상대방의 로프 연결 상태 감지
            bool isCurrentlyRoped = false;
            
            SpringJoint[] joints = rb.GetComponents<SpringJoint>();
            foreach (SpringJoint sj in joints)
            {
                if (sj.spring > 0f)
                {
                    isCurrentlyRoped = true;
                    break;
                }
            }

            if (!isCurrentlyRoped)
            {
                foreach (Rigidbody otherRb in rbs) 
                {
                    if (otherRb == rb) continue; 
                    
                    SpringJoint[] otherJoints = otherRb.GetComponents<SpringJoint>();
                    foreach (SpringJoint osj in otherJoints)
                    {
                        if (osj.spring > 0f && osj.connectedBody == rb)
                        {
                            isCurrentlyRoped = true;
                            break;
                        }
                    }
                    if (isCurrentlyRoped) break;
                }
            }

            if (!isCurrentlyRoped && playerWasRoped[rb])
            {
                playerJumpCounts[rb] = 0;
            }
            
            playerWasRoped[rb] = isCurrentlyRoped;

            bool shouldJump = false;

            // 3. MovingAst 스크립트에서 할당된 upKey를 읽어옵니다.
            MovingAst moveScript = rb.GetComponent<MovingAst>();
            if (moveScript != null)
            {
                if (Input.GetKeyDown(moveScript.upKey))
                {
                    shouldJump = true;
                }
            }

            // 4. 점프 버튼을 눌렀고 남은 점프 횟수가 있다면 날아오릅니다!
            if (shouldJump && playerJumpCounts[rb] < maxJumps)
            {
                playerJumpCounts[rb]++; 
                lastJumpTime[rb] = Time.time; // [추가됨] 방금 점프했다고 시간을 기록!
                
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
        }
    }

    bool IsGrounded(Rigidbody rb)
    {
        Collider col = rb.GetComponent<Collider>();
        if (col == null) return false;

        float distToGround = 1f;
        Vector3 center = col.bounds.center;

        if (col is CapsuleCollider cap)
        {
            distToGround = (cap.height / 2f) * rb.transform.lossyScale.y;
            center = rb.transform.TransformPoint(cap.center);
        }
        else if (col is BoxCollider box)
        {
            distToGround = (box.size.y / 2f) * rb.transform.lossyScale.y;
            center = rb.transform.TransformPoint(box.center);
        }
        else
        {
            distToGround = col.bounds.extents.y;
        }

        RaycastHit[] hits = Physics.RaycastAll(center, Vector3.down, distToGround + 0.15f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.root == rb.transform.root) continue;
            if (hit.collider.attachedRigidbody == rb) continue;
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
                lastJumpTime.Add(rb, 0f); // [추가됨] 장부에 시간 등록
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
                lastJumpTime.Remove(rb); // [추가됨] 장부에서 삭제
            }
        }
    }
}