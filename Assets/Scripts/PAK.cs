using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PAK : MonoBehaviour
{
    public float blinkSpeed = 2f;
    public float holdTime = 0.5f;
    
    public Vector3 targetPosition;
    public Vector3 targetRotation;
    
    public float duration = 2f; // 이동에 걸리는 시간 (초 단위)
    public string nextSceneName = "CharacterScene"; // 스크린샷에 있던 씬 이름으로 맞췄습니다!
    
    private TextMeshProUGUI textMeshPro;
    private bool isStarting = false;
    private Transform camTransform;
    
    // 시작 위치와 각도, 그리고 지나간 시간을 저장할 변수들
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float elapsedTime = 0f;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        // 텍스트 깜빡임 로직
        if (textMeshPro != null && !isStarting)
        {
            Color color = textMeshPro.color;
            float maxAlphaRange = 1f + (holdTime * blinkSpeed / 2f);
            float pingPongValue = Mathf.PingPong(Time.unscaledTime * blinkSpeed, maxAlphaRange);
            color.a = Mathf.Clamp01(pingPongValue);
            textMeshPro.color = color;
        }

        // 아무 키나 누르면 이동 시작 (시작 위치 기억하기)
        if (Input.anyKeyDown && !isStarting)
        {
            isStarting = true;
            if (textMeshPro != null) textMeshPro.enabled = false;
            // --- [여기에 추가!] 씬에 있는 모든 Trail Renderer를 찾아서 끕니다 ---
            TrailRenderer[] trails = FindObjectsOfType<TrailRenderer>();
            foreach (TrailRenderer trail in trails)
            {
                trail.enabled = false;
            }
            
            // 출발하는 순간의 카메라 위치와 각도를 변수에 저장합니다.
            startPosition = camTransform.position;
            startRotation = camTransform.rotation;
        }

        // 시간에 따른 부드러운 카메라 이동
        if (isStarting)
        {
            elapsedTime += Time.deltaTime; // 타이머를 굴립니다.
            
            // 지정한 시간(duration) 대비 현재 몇 퍼센트 왔는지 계산합니다. (0.0 ~ 1.0)
            float percentage = elapsedTime / duration;
            
            // SmoothStep을 사용해 시작과 끝이 부드러운 S자 곡선 비율을 만듭니다.
            float curve = Mathf.SmoothStep(0f, 1f, percentage);

            // 출발점부터 도착점까지 curve 비율에 맞춰 카메라를 옮깁니다.
            camTransform.position = Vector3.Lerp(startPosition, targetPosition, curve);
            
            Quaternion targetQuat = Quaternion.Euler(targetRotation);
            camTransform.rotation = Quaternion.Slerp(startRotation, targetQuat, curve);

            // 비율이 1(100%) 이상이 되면, 즉 지정한 시간이 꽉 차면 씬을 이동합니다.
            if (percentage >= 1f)
            {
                camTransform.position = targetPosition;
                camTransform.rotation = targetQuat;
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}