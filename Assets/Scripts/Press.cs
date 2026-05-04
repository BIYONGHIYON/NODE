using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위한 네임스페이스

public class TutorialTextController : MonoBehaviour
{
    // [수정됨] TextMeshProUGUI (UI용) 대신 TextMeshPro (3D 오브젝트용)을 사용합니다.
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
            textMeshPro = GetComponent<TextMeshPro>(); // [수정됨]

        // 1. 씬 시작 시, 과거에 튜토리얼을 깬 적이 있는지 검사합니다.
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            // 이미 깬 적이 있다면 텍스트를 아예 끄고 로직을 종료합니다.
            gameObject.SetActive(false);
            return;
        }
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

    // 3. 두 플레이어가 Hook을 연결했을 때 실행될 함수
    public void OnBothHooksConnected()
    {
        if (!isFadingOut && gameObject.activeSelf)
        {
            StartCoroutine(FadeOutAndSave());
        }
    }

    // 4. 서서히 사라지게 만들고 상태를 저장하는 코루틴
    private IEnumerator FadeOutAndSave()
    {
        isFadingOut = true; 
        
        Color startColor = textMeshPro.color;
        float startAlpha = startColor.a; 
        float time = 0f;

        // 설정한 시간 동안 알파값을 0을 향해 부드럽게 깎아냅니다.
        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            startColor.a = Mathf.Lerp(startAlpha, 0f, time / fadeOutDuration);
            textMeshPro.color = startColor;
            yield return null; 
        }

        // 완전히 투명해지도록 0으로 고정
        startColor.a = 0f;
        textMeshPro.color = startColor;

        // 5. 다음번 씬 입장 시 나타나지 않도록 기기에 저장합니다.
        //PlayerPrefs.SetInt(saveKey, 1);
        //PlayerPrefs.Save();

        // 텍스트 오브젝트 자체를 깔끔하게 비활성화
        gameObject.SetActive(false);
    }
}