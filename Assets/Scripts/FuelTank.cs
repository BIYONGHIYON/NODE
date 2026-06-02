using System.Collections;
using UnityEngine;

public class FuelTank : MonoBehaviour
{
    [Header("UI 설정")]
    public GameObject fuelAcquiredUI;
    public float textDisplayTime = 2f; 
    public float shrinkDuration = 0.5f;

    private bool isCollected = false;
    private Vector3 originalScale;

    void Start()
    {
        // 원래 크기 기억 및 UI 끄기
        originalScale = transform.localScale;
        
        if (fuelAcquiredUI != null)
        {
            fuelAcquiredUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 이미 누군가 먹고 연출이 진행 중이라면 무시합니다.
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            // 1. 나를 먹은 플레이어의 부활 스크립트를 찾습니다.
            PlayerRespawn playerScript = other.GetComponentInParent<PlayerRespawn>();
            
            // 2. 그 플레이어의 "개인 주머니"에 나를 추가합니다.
            if (playerScript != null && !playerScript.myCollectedTanks.Contains(this))
            {
                playerScript.myCollectedTanks.Add(this);
            }

            // 3. 바로 끄지 않고, 작아지면서 UI를 띄우는 예쁜 연출 코루틴을 실행합니다!
            StartCoroutine(CollectRoutine());
        }
    }

    IEnumerator CollectRoutine()
    {
        isCollected = true;
        GameData.isFuelAcquired = true;
        
        // UI 켜기
        if (fuelAcquiredUI != null) fuelAcquiredUI.SetActive(true);

        // 중복해서 먹지 못하도록 충돌체만 먼저 꺼줍니다.
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 부드럽게 작아지는 연출
        float elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }
        transform.localScale = Vector3.zero;

        // UI가 떠 있을 남은 시간만큼 대기
        float waitTime = Mathf.Max(0f, textDisplayTime - shrinkDuration);
        yield return new WaitForSeconds(waitTime);

        // UI 끄기
        if (fuelAcquiredUI != null) fuelAcquiredUI.SetActive(false);
        
        // 주의: Destroy나 SetActive(false)를 하지 않고 투명해진 상태(Scale 0)로 둡니다.
        // 그래야 죽었을 때 다시 ResetTank()를 불러서 크기를 키울 수 있습니다.
    }

    // 플레이어가 죽었을 때 PlayerRespawn 스크립트가 호출할 함수입니다.
    public void ResetTank()
    {
        StopAllCoroutines(); // 작아지던 중이거나 UI가 켜져있었다면 즉시 정지
        
        isCollected = false;
        GameData.isFuelAcquired = false;
        
        transform.localScale = originalScale; // 크기 원상복구
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true; // 다시 먹을 수 있게 켬
        
        if (fuelAcquiredUI != null) fuelAcquiredUI.SetActive(false);
    }
}