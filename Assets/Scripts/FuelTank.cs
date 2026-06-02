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
        originalScale = transform.localScale;
        
        if (fuelAcquiredUI != null)
        {
            fuelAcquiredUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerRespawn playerScript = other.GetComponentInParent<PlayerRespawn>();
            
            if (playerScript != null && !playerScript.myCollectedTanks.Contains(this))
            {
                playerScript.myCollectedTanks.Add(this);
            }

            
            StartCoroutine(CollectRoutine());
        }
    }

    IEnumerator CollectRoutine()
    {
        isCollected = true;
        GameData.isFuelAcquired = true;
        
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
    }

    public void ResetTank()
    {
        StopAllCoroutines();
        
        isCollected = false;
        GameData.isFuelAcquired = false;
        
        transform.localScale = originalScale; 
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        if (fuelAcquiredUI != null) fuelAcquiredUI.SetActive(false);
    }
}