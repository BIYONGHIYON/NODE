using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.EventSystems;

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

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        camTransform = Camera.main.transform;

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
            
            float pingPongValue = Mathf.PingPong(Time.unscaledTime * blinkSpeed, maxAlphaRange);
            color.a = Mathf.Clamp01(pingPongValue);
            textMeshPro.color = color;
        }

        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape) && !isStarting && Time.timeScale > 0f)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    return; 
                }
            }
            isStarting = true;

            if (sfxSource != null && cameraMoveSound != null)
            {
                sfxSource.PlayOneShot(cameraMoveSound);
            }
            
            if (textMeshPro != null) textMeshPro.enabled = false;
            if (titleImage != null) titleImage.SetActive(false);
            GameObject newGameBtn = GameObject.Find("NewGame");
            if (newGameBtn != null)
            {
                newGameBtn.SetActive(false);
            }
            
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