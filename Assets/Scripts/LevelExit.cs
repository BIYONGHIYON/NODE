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

    private static bool isTransitioning = false; 
    
    [HideInInspector]
    public List<GameObject> playersInZone = new List<GameObject>(); 

    private LevelExit[] allExits; 

    private Coroutine countdownCoroutine;
    private Coroutine warningCoroutine;

    void Awake()
    {
        isTransitioning = false; 

        allExits = FindObjectsOfType<LevelExit>();
    }

    void Start()
    {
        if (warningUI != null) warningUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTransitioning || !other.CompareTag("Player")) return;

        if (!playersInZone.Contains(other.gameObject))
        {
            playersInZone.Add(other.gameObject);
        }

        if (!GameData.isFuelAcquired)
        {
            StartWarning();
        }

        CheckGlobalCondition();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playersInZone.Contains(other.gameObject))
        {
            playersInZone.Remove(other.gameObject);
        }

        CheckGlobalCondition();
    }

    void CheckGlobalCondition()
    {
        if (isTransitioning) return;

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

        PlayerPrefs.SetInt("SaveProgress", GameData.currentProgress);
        PlayerPrefs.Save(); 

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