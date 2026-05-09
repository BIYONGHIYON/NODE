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

    // 상태를 저장할 고유 키값
    private string saveKey = "IsHookTutorialCleared"; 

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshPro>();

        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
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

        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();

        gameObject.SetActive(false);
    }
}