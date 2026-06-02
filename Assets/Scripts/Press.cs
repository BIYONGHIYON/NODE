using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialTextController : MonoBehaviour
{
    public TextMeshPro textMeshPro; 
    public float blinkSpeed = 2f;
    public float holdTime = 1f;
    
    [Header("Fade Out Settings")]
    public float fadeOutDuration = 1.5f;

    private bool isStarting = false;
    private bool isFadingOut = false; 

    private static bool isTutorialCleared = false; 

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshPro>();

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

        isTutorialCleared = true;

        gameObject.SetActive(false);
    }
}