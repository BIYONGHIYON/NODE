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
    private Dictionary<Rigidbody, float> lastJumpTime = new Dictionary<Rigidbody, float>(); 

    void Update()
    {
        List<Rigidbody> rbs = new List<Rigidbody>(playerJumpCounts.Keys);

        foreach (Rigidbody rb in rbs)
        {
            if (rb == null || rb.isKinematic) continue;

            if (Time.time - lastJumpTime[rb] > 0.2f && rb.velocity.y <= 0.1f && IsGrounded(rb))
            {
                playerJumpCounts[rb] = 0;
            }

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

            MovingAst moveScript = rb.GetComponent<MovingAst>();
            if (moveScript != null)
            {
                if (Input.GetKeyDown(moveScript.upKey))
                {
                    shouldJump = true;
                }
            }

            if (shouldJump && playerJumpCounts[rb] < maxJumps)
            {
                playerJumpCounts[rb]++; 
                lastJumpTime[rb] = Time.time; 
                
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
        }
    }

    // ========================================================
    // [버그 수정] 속도(Velocity) 검사를 빼고 무조건 진짜 바닥인지 이중 검사
    // ========================================================
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

        // 1차 레이저: 플레이어 발밑 검사
        RaycastHit[] hits = Physics.RaycastAll(center, Vector3.down, distToGround + 0.15f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            // 나 자신과 관련된 오브젝트 무시
            if (hit.transform.root == rb.transform.root) continue;
            if (hit.collider.attachedRigidbody == rb) continue;
            if (hit.collider.CompareTag("Player")) continue;

            // --------------------------------------------------------
            // 밟은 물체가 움직이는 돌(Rock 등)일 경우
            // --------------------------------------------------------
            Rigidbody hitRb = hit.collider.attachedRigidbody;
            if (hitRb != null && !hitRb.isKinematic)
            {
                // 돌의 최고점(속도=0) 꼼수를 막기 위해 무조건 이중 검사 실시!
                float rockDistToGround = hit.collider.bounds.extents.y;
                Vector3 rockCenter = hit.collider.bounds.center;
                
                // 2차 레이저: 돌의 중심에서 바닥을 향해 발사
                RaycastHit[] rockHits = Physics.RaycastAll(rockCenter, Vector3.down, rockDistToGround + 0.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                
                bool isRockGrounded = false;
                foreach (RaycastHit rockHit in rockHits)
                {
                    // 돌 자신, 플레이어를 제외한 무언가(진짜 바닥)에 닿아있는지 확인
                    if (rockHit.collider == hit.collider) continue;
                    if (rockHit.transform.root == hitRb.transform.root) continue;
                    if (rockHit.collider.CompareTag("Player")) continue;
                    
                    isRockGrounded = true;
                    break;
                }

                // 돌 밑에 진짜 바닥이 없다면, 이 돌은 공중에 떠 있는 가짜 바닥이므로 무시!
                if (!isRockGrounded)
                {
                    continue; 
                }
            }
            // --------------------------------------------------------

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
                lastJumpTime.Add(rb, 0f); 
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
                lastJumpTime.Remove(rb); 
            }
        }
    }
}