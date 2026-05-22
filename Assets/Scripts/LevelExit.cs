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
    
    // 외부(다른 출구)에서 몇 명이나 들어왔는지 확인할 수 있게 숨김 처리 후 public 전환
    [HideInInspector]
    public List<GameObject> playersInZone = new List<GameObject>(); 

    private static LevelExit[] allExits; // 씬 내의 모든 탈출구 목록
    private Coroutine countdownCoroutine;
    private Coroutine warningCoroutine;

    void Awake()
    {
        // 씬이 로드될 때 전역 변수 초기화
        isTransitioning = false; 
    }

    void Start()
    {
        if (warningUI != null) warningUI.SetActive(false);
        
        // 시작할 때 씬에 있는 모든 탈출구를 자동으로 찾아서 배열로 저장
        allExits = FindObjectsOfType<LevelExit>();
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

        // 누군가 나갔으니 조건이 깨졌는지(1초 대기 취소) 확인
        CheckGlobalCondition();
    }

    void CheckGlobalCondition()
    {
        if (isTransitioning) return;

        bool conditionMet = false;

        // ========================================================
        // [핵심] 출구 개수에 따른 자동 조건 분기
        // ========================================================
        if (allExits.Length == 1)
        {
            // 출구가 1개뿐이라면: 두 명이 모두 이 구역에 있어야 함
            conditionMet = (playersInZone.Count >= 2);
        }
        else if (allExits.Length >= 2)
        {
            // 출구가 2개 이상이라면: 첫 번째 출구와 두 번째 출구 모두 1명 이상씩 있어야 함
            conditionMet = (allExits[0].playersInZone.Count >= 1 && allExits[1].playersInZone.Count >= 1);
        }

        // 조건과 연료통을 모두 만족했다면 1초 카운트다운 시작
        if (conditionMet && GameData.isFuelAcquired)
        {
            StartCountdownOnAll();
        }
        else
        {
            // 한 명이라도 1초가 되기 전에 밖으로 나가면 즉시 카운트다운 취소!
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
        // 인스펙터에서 설정한 시간(1초)만큼 대기
        yield return new WaitForSeconds(waitTime);

        // 1초가 무사히 지났다면 클리어 처리 진행
        if (isTransitioning) yield break;
        isTransitioning = true;

        GameData.currentProgress = progressToSetOnClear; 
        GameData.isFuelAcquired = false; 

        PlanetClearManager clearManager = FindObjectOfType<PlanetClearManager>();
        if (clearManager != null)
        {
            clearManager.ReturnToTutorial();
        }
    }

    public void StartWarning()
    {
        // 경고가 이미 켜져 있다면 껐다 다시 켬 (깜빡임 방지)
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

    // 모든 탈출구의 타이머를 동시에 조작하기 위한 헬퍼 함수
    void StartCountdownOnAll()
    {
        foreach (var exit in allExits)
        {
            exit.StartCountdown();
        }
    }

    void StopCountdownOnAll()
    {
        foreach (var exit in allExits)
        {
            exit.StopCountdown();
        }
    }
}