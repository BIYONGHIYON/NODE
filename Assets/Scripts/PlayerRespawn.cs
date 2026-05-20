using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("체크포인트 위치")]
    public Vector3 currentCheckpoint;

    [Header("사망 연출 설정")]
    public float deathDuration = 0.5f;     
    public float shakeMagnitude = 0.3f;    
    public float respawnDuration = 0.5f;   

    private Rigidbody rb;
    private RopeAction ropeAction;
    
    private Vector3 originalScale;
    private bool isDead = false; 

    void Start()
    {
        currentCheckpoint = transform.position;
        originalScale = transform.localScale; 
        
        rb = GetComponent<Rigidbody>();
        ropeAction = GetComponent<RopeAction>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("DeathZone"))
        {
            StartCoroutine(DieAndRespawnRoutine());
        }
        else if (other.CompareTag("Checkpoint"))
        {
            currentCheckpoint = other.transform.position;
            
            // ========================================================
            // [추가됨] 체크포인트를 무사히 찍었으니, 
            // 방금까지 먹었던 연료통들을 진짜로 획득 처리(영구 파괴)합니다!
            foreach(FuelTank tank in FuelTank.recentlyCollected)
            {
                if (tank != null) Destroy(tank.gameObject);
            }
            FuelTank.recentlyCollected.Clear(); // 보관함 비우기
            // ========================================================
        }
    }

    IEnumerator DieAndRespawnRoutine()
    {
        isDead = true;

        if (!rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
        }
        rb.isKinematic = true;

        if (ropeAction != null)
        {
            ropeAction.isHoldingKey = false;
            ropeAction.DetachPlayer(); 
        }

        // ========================================================
        // [추가됨] 죽었으니 체크포인트를 찍기 전까지 먹었던 연료통을 
        // 전부 제자리로 뱉어냅니다 (원상복구)!
        foreach(FuelTank tank in FuelTank.recentlyCollected)
        {
            if (tank != null) tank.ResetTank();
        }
        FuelTank.recentlyCollected.Clear(); // 보관함 비우기
        // ========================================================

        // 1단계: 흔들림 + 작아짐
        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathDuration;
            
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);

            yield return null;
        }

        transform.localScale = Vector3.zero;
        transform.position = originalPos; 

        // 2단계: 체크포인트로 텔레포트 
        transform.position = currentCheckpoint;
        yield return new WaitForSeconds(0.2f); 

        // 3단계: 다시 커지면서 부활
        elapsed = 0f;
        while (elapsed < respawnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / respawnDuration;
            
            float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easeProgress);
            
            yield return null;
        }

        transform.localScale = originalScale;
        rb.isKinematic = false;
        isDead = false;
    }
}