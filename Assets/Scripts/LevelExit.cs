using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("연료통이 없을 때 띄울 경고 문구 UI를 연결하세요.")]
    public GameObject warningUI; 
    public float warningDuration = 2f;

    [Header("클리어 설정")]
    [Tooltip("이 행성을 클리어했을 때 도달하게 될 진행도 숫자입니다.")]
    public int progressToSetOnClear = 2;
    
    [Tooltip("클리어 조건을 만족하고 대기해야 하는 시간(초)입니다.")]
    public float waitTime = 1f;

    // 씬 내의 다른 출구들과 중복 실행을 막기 위해 static(전역) 변수로 선언
    private static bool isTransitioning = false; 
    
    [HideInInspector]
    public List<GameObject> playersInZone = new List<GameObject>(); 

    // ========================================================
    // [버그 수정] static을 제거하여 씬이 바뀔 때마다 깔끔하게 초기화되도록 합니다!
    private LevelExit[] allExits; 
    // ========================================================

    private Coroutine countdownCoroutine;
    private Coroutine warningCoroutine;

    void Awake()
    {
        // 씬이 로드될 때 전역 변수 초기화
        isTransitioning = false; 

        // [버그 수정] Start보다 무조건 먼저 실행되는 Awake에서 출구들을 미리 찾아둡니다!
        // 플레이어가 시작하자마자 출구에 닿아도 에러가 나지 않습니다.
        allExits = FindObjectsOfType<LevelExit>();
    }

    void Start()
    {
        if (warningUI != null) warningUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTransitioning || !other.CompareTag("Player")) return;

        // 플레이어 명단 추가
        if (!playersInZone.Contains(other.gameObject))
        {
            playersInZone.Add(other.gameObject);
        }

        // 연료가 없을 때 들어오면 경고 문구 즉시 출력
        if (!GameData.isFuelAcquired)
        {
            StartWarning();
        }

        CheckGlobalCondition();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 플레이어가 나가면 명단에서 제거
        if (playersInZone.Contains(other.gameObject))
        {
            playersInZone.Remove(other.gameObject);
        }

        CheckGlobalCondition();
    }

    void CheckGlobalCondition()
    {
        if (isTransitioning) return;

        // [이중 안전장치] 혹시라도 출구 배열이 비어있거나, 이전 씬의 찌꺼기가 남아 파괴된 상태(null)라면 다시 찾습니다.
        if (allExits == null || allExits.Length == 0 || allExits[0] == null)
        {
            allExits = FindObjectsOfType<LevelExit>();
        }

        bool conditionMet = false;

        if (allExits.Length == 1)
        {
            conditionMet = (playersInZone.Count >= 2);
        }
        else if (allExits.Length >= 2)
        {
            conditionMet = (allExits[0].playersInZone.Count >= 1 && allExits[1].playersInZone.Count >= 1);
        }

        if (conditionMet && GameData.isFuelAcquired)
        {
            StartCountdownOnAll();
        }
        else
        {
            StopCountdownOnAll();
        }
    }

    // ========================================================
    // 타이머 및 연출 코루틴
    // ========================================================
    public void StartCountdown()
    {
        if (countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(CountdownRoutine());
        }
    }

    public void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }

    private IEnumerator CountdownRoutine()
    {
        yield return new WaitForSeconds(waitTime);

        if (isTransitioning) yield break;
        isTransitioning = true;

        GameData.currentProgress = progressToSetOnClear; 
        GameData.isFuelAcquired = false;
        GameData.justClearedPlanet = true;

        PlanetClearManager clearManager = FindObjectOfType<PlanetClearManager>();
        if (clearManager != null)
        {
            clearManager.ReturnToTutorial();
        }
    }

    public void StartWarning()
    {
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(ShowWarningRoutine());
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

    void StartCountdownOnAll()
    {
        if (allExits == null) return;
        foreach (var exit in allExits)
        {
            if (exit != null) exit.StartCountdown();
        }
    }

    void StopCountdownOnAll()
    {
        if (allExits == null) return;
        foreach (var exit in allExits)
        {
            if (exit != null) exit.StopCountdown();
        }
    }
}