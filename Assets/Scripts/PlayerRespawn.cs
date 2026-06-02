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

        foreach(FuelTank tank in myCollectedTanks)
        {
            if (tank != null) tank.ResetTank();
        }
        myCollectedTanks.Clear(); 

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

        transform.position = currentCheckpoint;
        yield return new WaitForSeconds(0.2f); 

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