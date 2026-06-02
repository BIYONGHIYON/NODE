using System.Collections;
using UnityEngine;
using TMPro;

public class press1 : MonoBehaviour
{
    public TextMeshPro textMeshPro; 
    public float blinkSpeed = 2f;
    public float holdTime = 1f;
    
    [Header("Fade Out Settings")]
    public float fadeOutDuration = 1.5f;

    private bool isStarting = false;
    private bool isFadingOut = false; 

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshPro>();
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
}