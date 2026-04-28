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
    public GameObject titleImage; // [추가됨] 타이틀 이미지 오브젝트를 껐다 켤 변수

    private TextMeshProUGUI textMeshPro;
    private bool isStarting = false;
    private Transform camTransform;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float elapsedTime = 0f;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        camTransform = Camera.main.transform;

        // 게임이 시작될 때 영상이 켜져 있다면, 텍스트와 타이틀 이미지를 모두 숨겨둡니다.
        if (introVideo != null && introVideo.enabled)
        {
            if (textMeshPro != null) textMeshPro.enabled = false;
            if (titleImage != null) titleImage.SetActive(false); // 타이틀 숨기기
        }
    }

    void Update()
    {
        // 비디오가 재생 중이면 아래 코드는 무시하고 대기합니다.
        if (introVideo != null && introVideo.enabled)
        {
            return;
        }

        // 비디오가 끝났는데 UI가 아직 꺼져 있다면 다시 화면에 켭니다.
        if (textMeshPro != null && !textMeshPro.enabled && !isStarting)
        {
            textMeshPro.enabled = true;
            if (titleImage != null) titleImage.SetActive(true); // 타이틀 다시 켜기
        }

        // 텍스트 깜빡임 로직
        if (textMeshPro != null && !isStarting)
        {
            Color color = textMeshPro.color;
            float maxAlphaRange = 1f + (holdTime * blinkSpeed / 2f);
            float pingPongValue = Mathf.PingPong(Time.unscaledTime * blinkSpeed, maxAlphaRange);
            color.a = Mathf.Clamp01(pingPongValue);
            textMeshPro.color = color;
        }

        // 아무 키나 누르면 이동 시작
        if (Input.anyKeyDown && !isStarting)
        {
            isStarting = true;
            
            // 씬 이동을 시작할 때 깔끔하게 보이도록 UI를 모두 끕니다.
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

        // 카메라 부드러운 이동 로직
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