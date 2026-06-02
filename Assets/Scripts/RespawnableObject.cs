using System.Collections;
using System.Reflection; // [추가됨] 숨겨진 변수를 찾기 위한 기능
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RespawnableObject : MonoBehaviour
{
    [Header("리스폰 연출 설정")]
    public float deathDuration = 0.5f;     
    public float shakeMagnitude = 0.1f;    
    public float respawnDuration = 0.5f;   

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;         
    
    private Rigidbody rb;
    private bool isRespawning = false;     

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale; 
        
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return;
        if (other.CompareTag("DeathZone"))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isRespawning) return;
        if (collision.gameObject.CompareTag("DeathZone"))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    public void ResetToStart()
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    // ========================================================
    // [추가됨] 나에게 연결된 모든 로프를 찾아 강제로 끊는 함수
    // ========================================================
    private void DisconnectAllRopes()
    {
        // 씬에 있는 모든 로프 액션 스크립트를 찾습니다.
        RopeAction[] allRopes = FindObjectsOfType<RopeAction>();
        
        foreach (RopeAction rope in allRopes)
        {
            bool isAttached = false;

            // 1. 이미 물리적으로 찰칵! 연결된 상태인지 검사
            SpringJoint[] joints = rope.GetComponents<SpringJoint>();
            foreach (SpringJoint sj in joints)
            {
                if (sj != null && sj.connectedBody == rb)
                {
                    isAttached = true;
                    break;
                }
            }

            // 2. 만약 플레이어가 쏴서 로프가 내 쪽으로 '날아오고 있는 중'이라면?
            // (RopeAction의 private 변수인 targetAnchor를 안전하게 몰래 확인합니다)
            if (!isAttached)
            {
                FieldInfo field = typeof(RopeAction).GetField("targetAnchor", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    Transform target = field.GetValue(rope) as Transform;
                    if (target == this.transform)
                    {
                        isAttached = true;
                    }
                }
            }

            // 나에게 연결되어 있거나 날아오는 중이라면, "당장 로프 풀어!" 라고 명령합니다.
            if (isAttached)
            {
                rope.SendMessage("DetachAnchor", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // ========================================================
        // [추가됨] 돌이 작아지기 전에, 연결된 플레이어의 로프부터 싹둑 자릅니다!
        // ========================================================
        DisconnectAllRopes();

        // 1. 떨어지던 물리적인 힘을 즉시 0으로 만들고, 허공에 멈춰 세웁니다.
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; 

        // 2단계: 부들부들 흔들리며 작아짐
        Vector3 currentPos = transform.position;
        float elapsed = 0f;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathDuration;
            
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = currentPos + new Vector3(offsetX, offsetY, 0);

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);

            yield return null;
        }

        // 3단계: 크기를 완전히 0으로 만들고, 처음 위치로 순간이동
        transform.localScale = Vector3.zero;
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        yield return new WaitForSeconds(0.2f); 

        // 4단계: 다시 커지면서 부활
        elapsed = 0f;
        while (elapsed < respawnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / respawnDuration;
            
            float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easeProgress);
            
            yield return null;
        }

        // 5단계: 원상 복구 및 물리 효과 재개
        transform.localScale = originalScale;
        rb.isKinematic = false;
        isRespawning = false;
    }
}