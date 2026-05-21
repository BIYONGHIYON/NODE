using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("연료통이 없을 때 띄울 경고 문구 UI를 연결하세요.")]
    public GameObject warningUI; 
    
    public float warningDuration = 2f;

    [Header("클리어 설정")]
    [Tooltip("이 행성을 클리어했을 때 도달하게 될 진행도 숫자입니다.")]
    public int progressToSetOnClear = 2;

    private bool isTransitioning = false;
    private List<GameObject> playersInZone = new List<GameObject>();

    void Start()
    {
        if (warningUI != null)
        {
            warningUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTransitioning || !other.CompareTag("Player")) return;

        if (!playersInZone.Contains(other.gameObject))
        {
            playersInZone.Add(other.gameObject);
        }

        CheckConditionAndExit();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playersInZone.Contains(other.gameObject))
        {
            playersInZone.Remove(other.gameObject);
        }
    }

    void CheckConditionAndExit()
    {
        if (!GameData.isFuelAcquired)
        {
            StartCoroutine(ShowWarningRoutine());
            return;
        }

        if (GameData.isFuelAcquired && playersInZone.Count >= 2)
        {
            if (isTransitioning) return;
            isTransitioning = true;

            // [수정 포인트] 다음 씬을 위해 획득 상태 초기화
            // (currentProgress 숫자는 인스펙터에서 설정한 대로 1 -> 2로 갱신됩니다)
            GameData.currentProgress = progressToSetOnClear; 
            GameData.isFuelAcquired = false; 

            // ========================================================
            // 새롭게 만든 만능 클리어 매니저를 찾아서 실행시킵니다!
            PlanetClearManager clearManager = FindObjectOfType<PlanetClearManager>();
            
            if (clearManager != null)
            {
                clearManager.ReturnToTutorial();
            }
            // ========================================================
        }
    }

    IEnumerator ShowWarningRoutine()
    {
        if (warningUI != null)
        {
            warningUI.SetActive(true);
            yield return new WaitForSeconds(warningDuration);
            warningUI.SetActive(false);
        }
    }
}