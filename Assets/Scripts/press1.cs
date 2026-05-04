using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위한 네임스페이스

public class press1 : MonoBehaviour
{
    // [수정됨] TextMeshProUGUI (UI용) 대신 TextMeshPro (3D 오브젝트용)을 사용합니다.
    public TextMeshPro textMeshPro; 
    public float blinkSpeed = 2f;
    public float holdTime = 1f;
    
    [Header("Fade Out Settings")]
    public float fadeOutDuration = 1.5f; // 서서히 사라지는데 걸리는 시간(초)

    private bool isStarting = false;
    private bool isFadingOut = false; // 페이드 아웃이 시작되었는지 체크하는 플래그

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshPro>(); // [수정됨]
    }

    void Update()
    {
        // 2. 평소 깜빡임 로직
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