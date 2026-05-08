using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video; 

public class PAK : MonoBehaviour
{
    public float blinkSpeed = 2f;
    public float holdTime = 0.5f;
    
    public Vector3 targetPosition;
    public Vector3 targetRotation;
    
    public float duration = 2f; 
    public string nextSceneName = "CharacterScene"; 
    
    [Header("Video & UI Settings")]
    public VideoPlayer introVideo; 
    public GameObject titleImage; 

    [Header("SFX Settings")]
    public AudioSource sfxSource;
    public AudioClip cameraMoveSound;

    private TextMeshProUGUI textMeshPro;
    private bool isStarting = false;
    private Transform camTransform;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float elapsedTime = 0f;

    // 기존 PAK.cs의 Start() 함수 내부만 이렇게 수정해 주세요.
    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        camTransform = Camera.main.transform;

        // [추가된 부분] 씬이 이동하면서 VideoPlayer 참조가 끊겼다면 유지되고 있는 메인 카메라에서 직접 찾습니다.
        if (introVideo == null && Camera.main != null)
        {
            introVideo = Camera.main.GetComponent<VideoPlayer>();
        }

        if (introVideo != null && introVideo.enabled)
        {
            if (textMeshPro != null) textMeshPro.enabled = false;
            if (titleImage != null) titleImage.SetActive(false); 
        }
    }

    void Update()
    {
        if (introVideo != null && introVideo.enabled)
        {
            return;
        }

        if (textMeshPro != null && !textMeshPro.enabled && !isStarting)
        {
            textMeshPro.enabled = true;
            if (titleImage != null) titleImage.SetActive(true); 
        }

        if (textMeshPro != null && !isStarting)
        {
            Color color = textMeshPro.color;
            float maxAlphaRange = 1f + (holdTime * blinkSpeed / 2f);
            
            // Time.unscaledTime을 사용하므로 일시 정지(timeScale = 0) 상태에서도 텍스트는 정상적으로 깜빡입니다.
            float pingPongValue = Mathf.PingPong(Time.unscaledTime * blinkSpeed, maxAlphaRange);
            color.a = Mathf.Clamp01(pingPongValue);
            textMeshPro.color = color;
        }

        // ==========================================
        // [수정됨] Time.timeScale > 0f 조건을 추가하여, 메뉴가 켜져서 시간이 멈춘 상태에서는 입력을 완전히 무시합니다.
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape) && !isStarting && Time.timeScale > 0f)
        {
        // ==========================================
            isStarting = true;

            if (sfxSource != null && cameraMoveSound != null)
            {
                sfxSource.PlayOneShot(cameraMoveSound);
            }
            
            if (textMeshPro != null) textMeshPro.enabled = false;
            if (titleImage != null) titleImage.SetActive(false);
            
            TrailRenderer[] trails = FindObjectsOfType<TrailRenderer>();
            foreach (TrailRenderer trail in trails)
            {
                trail.enabled = false;
            }
            
            startPosition = camTransform.position;
            startRotation = camTransform.rotation;
        }

        if (isStarting)
        {
            elapsedTime += Time.deltaTime; 
            
            float percentage = elapsedTime / duration;
            float curve = Mathf.SmoothStep(0f, 1f, percentage);

            camTransform.position = Vector3.Lerp(startPosition, targetPosition, curve);
            
            Quaternion targetQuat = Quaternion.Euler(targetRotation);
            camTransform.rotation = Quaternion.Slerp(startRotation, targetQuat, curve);

            if (percentage >= 1f)
            {
                camTransform.position = targetPosition;
                camTransform.rotation = targetQuat;
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}