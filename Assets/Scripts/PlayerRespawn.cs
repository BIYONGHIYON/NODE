using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("체크포인트 위치")]
    public Vector3 currentCheckpoint;

    [Header("사망 연출 설정")]
    public float deathDuration = 0.5f;     
    public float shakeMagnitude = 0.3f;    
    public float respawnDuration = 0.5f;   

    // 나만의 개인 연료통 주머니
    public List<FuelTank> myCollectedTanks = new List<FuelTank>();

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
            // ========================================================
            // [버그 수정] 다른 플레이어 간섭 없이 '나'의 위치와 주머니만 갱신!
            // ========================================================
            currentCheckpoint = other.transform.position;
            
            foreach(FuelTank tank in myCollectedTanks)
            {
                if (tank != null) Destroy(tank.gameObject); // 세이브 완료 (파괴)
            }
            myCollectedTanks.Clear(); // 내 주머니만 비우기
        }
    }

    IEnumerator DieAndRespawnRoutine()
    {
        isDead = true;

        if (!rb.isKinematic) rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        if (ropeAction != null) ropeAction.CutAllRopes();

        // 내 주머니에 있는 연료통만 원래 자리로 되돌림
        foreach(FuelTank tank in myCollectedTanks)
        {
            if (tank != null) tank.ResetTank();
        }
        myCollectedTanks.Clear(); 

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

        // 2단계: 내 전용 체크포인트로 텔레포트 
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