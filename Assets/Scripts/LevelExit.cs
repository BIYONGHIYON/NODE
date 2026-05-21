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
        // 1. 연료통을 획득하지 못했다면 경고
        if (!GameData.isFuelAcquired)
        {
            StartCoroutine(ShowWarningRoutine());
            return;
        }

        // 2. 연료통을 획득했고 두 명이 모두 들어왔다면 탈출!
        if (GameData.isFuelAcquired && playersInZone.Count >= 2)
        {
            if (isTransitioning) return;
            isTransitioning = true;

            // 진행도를 업데이트하고 획득 상태를 초기화합니다.
            GameData.currentProgress = 1;
            GameData.isFuelAcquired = false; 

            // [핵심] 이미 씬에 존재하는 GameMenuManager를 찾아서, 
            // 정상 작동하는 ReturnToShip(페이드아웃 및 카메라 이동) 로직을 대신 실행시킵니다!
            GameMenuManager menuManager = FindObjectOfType<GameMenuManager>();
            
            if (menuManager != null)
            {
                menuManager.ReturnToShip();
            }
            else
            {
                // 혹시라도 매니저를 찾지 못했을 때를 대비한 안전 장치
                Debug.LogWarning("GameMenuManager를 찾지 못해 기본 방식으로 이동합니다.");
                SceneManager.LoadScene("TutorialScene");
            }
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