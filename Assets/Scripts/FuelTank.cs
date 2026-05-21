using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelTank : MonoBehaviour
{
    [Header("UI 설정")]
    public GameObject fuelAcquiredUI;
    public float textDisplayTime = 2f; 
    public float shrinkDuration = 0.5f;

    private bool isCollected = false;
    private Vector3 originalScale;

    // [핵심] 체크포인트를 찍기 전까지 '임시로' 먹은 연료통들을 기억하는 공유 보관함입니다.
    public static List<FuelTank> recentlyCollected = new List<FuelTank>();

    void Start()
    {
        recentlyCollected.Clear(); // 씬이 시작될 때 찌꺼기가 남아있지 않도록 싹 비워줍니다.
        originalScale = transform.localScale;
        
        if (fuelAcquiredUI != null)
        {
            fuelAcquiredUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 태그가 "Player"인 오브젝트가 닿았을 때만 작동!
        if (!isCollected && other.CompareTag("Player"))
        {
            StartCoroutine(CollectRoutine());
        }
    }

    IEnumerator CollectRoutine()
    {
        isCollected = true;
        GameData.isFuelAcquired = true;
        
        // 보관함에 이 연료통을 등록해둡니다. (죽으면 뱉어내기 위해)
        recentlyCollected.Add(this); 

        if (fuelAcquiredUI != null) fuelAcquiredUI.SetActive(true);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        float elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }
        transform.localScale = Vector3.zero;

        float waitTime = Mathf.Max(0f, textDisplayTime - shrinkDuration);
        yield return new WaitForSeconds(waitTime);

        if (fuelAcquiredUI != null) fuelAcquiredUI.SetActive(false);
        
        // 🚨 여기서 Destroy(gameObject)를 하지 않고 투명하게 숨겨만 둡니다!
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