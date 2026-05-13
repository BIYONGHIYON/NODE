using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialTextController : MonoBehaviour
{
    public TextMeshPro textMeshPro; 
    public float blinkSpeed = 2f;
    public float holdTime = 1f;
    
    [Header("Fade Out Settings")]
    public float fadeOutDuration = 1.5f; // 서서히 사라지는데 걸리는 시간(초)

    private bool isStarting = false;
    private bool isFadingOut = false; // 페이드 아웃이 시작되었는지 체크하는 플래그

    // PlayerPrefs 대신 static 변수 사용 (게임 실행 중에만 유지, 껐다 켜면 초기화됨)
    private static bool isTutorialCleared = false; 

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshPro>();

        // static 변수를 확인하여 이미 클리어했다면 비활성화
        if (isTutorialCleared)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void Update()
    {
        if (textMeshPro != null && !isStarting && !isFadingOut)
        {
            Color color = textMeshPro.color;
            float maxAlphaRange = 1f + (holdTime * blinkSpeed / 2f);
            float pingPongValue = Mathf.PingPong(Time.unscaledTime * blinkSpeed, maxAlphaRange);
            color.a = Mathf.Clamp01(pingPongValue);
            textMeshPro.color = color;
        }
    }

    public void OnBothHooksConnected()
    {
        if (!isFadingOut && gameObject.activeSelf)
        {
            StartCoroutine(FadeOutAndSave());
        }
    }

    private IEnumerator FadeOutAndSave()
    {
        isFadingOut = true; 
        
        Color startColor = textMeshPro.color;
        float startAlpha = startColor.a; 
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            startColor.a = Mathf.Lerp(startAlpha, 0f, time / fadeOutDuration);
            textMeshPro.color = startColor;
            yield return null; 
        }

        startColor.a = 0f;
        textMeshPro.color = startColor;

        // 튜토리얼 클리어 상태를 true로 변경
        isTutorialCleared = true;

        gameObject.SetActive(false);
    }
}