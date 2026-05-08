using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using UnityEngine.Video; 
using System.Collections; 

public class MapSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform[] planetNodes; 
    public RectTransform selectorCursor; 
    
    [Header("Error UI Settings")]
    public GameObject errorPopupUI; 

    [Header("Settings")]
    public float cursorMoveSpeed = 10f; 
    public Vector3 exitCursorOffset = new Vector3(0f, -15f, 0f); 

    [Header("Progress Settings")]
    [Tooltip("현재 진행도 (인스펙터의 값은 실행 시 GameData 값으로 덮어씌워집니다.)")]
    public int currentProgress = 0; 
    
    [Tooltip("각 노드(행성)별 필요한 진행도 수치. planetNodes 배열과 순서/개수를 맞춰주세요.")]
    public int[] requiredProgress; 

    [Header("Fade Settings")]
    public float fadeDuration = 1f; 

    [Header("Scene Settings")]
    public string[] planetSceneNames; 
    
    // ==========================================
    // [수정됨] 배열을 없애고 단일 비디오 클립만 받도록 변경
    [Header("Video Settings")]
    [Tooltip("모든 행성 이동 시 재생할 공통 영상을 넣어주세요.")]
    public VideoClip transitionVideo; 
    // ==========================================

    [Header("System References")]
    public InteractionBox interactionBox; 

    [Header("SFX Settings")]
    public AudioSource sfxSource;
    public AudioClip successSound; 
    public AudioClip errorSound;   

    private int currentIndex = 0;
    private bool isTransitioning = false; 
    private Coroutine errorCoroutine; 

    void OnEnable()
    {
        currentProgress = GameData.currentProgress;

        isTransitioning = false;
        currentIndex = 0; 
        
        if (errorPopupUI != null) errorPopupUI.SetActive(false);

        for (int i = 0; i < planetSceneNames.Length; i++)
        {
            if (planetSceneNames[i].ToLower() == "exit")
            {
                currentIndex = i;
                break; 
            }
        }

        if (planetNodes.Length > 0 && selectorCursor != null)
        {
            selectorCursor.position = GetTargetPosition(currentIndex);
        }
    }

    void Update()
    {
        if (isTransitioning) return; 

        HandleInput();
        UpdateCursorPosition();
    }

    void HandleInput()
    {
        if (errorPopupUI != null && errorPopupUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                if (errorCoroutine != null) StopCoroutine(errorCoroutine);
                errorPopupUI.SetActive(false);
                return; 
            }
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0) 
                currentIndex = planetNodes.Length - 1; 
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            if (currentIndex >= planetNodes.Length) 
                currentIndex = 0; 
        }

        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            SelectPlanet();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMap();
        }
    }

    void UpdateCursorPosition()
    {
        if (planetNodes.Length == 0 || selectorCursor == null) return;

        Vector3 targetPos = GetTargetPosition(currentIndex);
        selectorCursor.position = Vector3.Lerp(selectorCursor.position, targetPos, Time.deltaTime * cursorMoveSpeed);
    }

    private Vector3 GetTargetPosition(int index)
    {
        Vector3 pos = planetNodes[index].position;
        if (index < planetSceneNames.Length && planetSceneNames[index].ToLower() == "exit")
        {
            pos += exitCursorOffset;
        }
        return pos;
    }

    void SelectPlanet()
    {
        if (currentIndex < planetSceneNames.Length)
        {
            string targetName = planetSceneNames[currentIndex];

            if (targetName == "Exit" || targetName == "exit")
            {
                CloseMap();
                return; 
            }

            int required = 0;
            if (requiredProgress != null && currentIndex < requiredProgress.Length)
            {
                required = requiredProgress[currentIndex];
            }

            if (currentProgress >= required)
            {
                if (sfxSource != null && successSound != null)
                {
                    sfxSource.PlayOneShot(successSound);
                }

                if (!string.IsNullOrEmpty(targetName))
                {
                    isTransitioning = true;
                    
                    GameObject helperObj = new GameObject("TransitionHelper");
                    SceneTransitionHelper helper = helperObj.AddComponent<SceneTransitionHelper>();
                    
                    // ==========================================
                    // [수정됨] 공통으로 설정된 transitionVideo 하나만 전달합니다.
                    helper.StartCoroutine(helper.Transition(targetName, fadeDuration, transitionVideo));
                    // ==========================================
                }
            }
            else
            {
                if (sfxSource != null && errorSound != null)
                {
                    sfxSource.PlayOneShot(errorSound);
                }

                if (errorCoroutine != null) StopCoroutine(errorCoroutine);
                errorCoroutine = StartCoroutine(ShowErrorPopup());
            }
        }
    }

    private IEnumerator ShowErrorPopup()
    {
        if (errorPopupUI != null)
        {
            errorPopupUI.SetActive(true);
            yield return new WaitForSeconds(2f);
            errorPopupUI.SetActive(false);
        }
    }

    public void CloseMap() 
    {
        if (interactionBox != null)
        {
            interactionBox.EndInteraction(); 
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

public class SceneTransitionHelper : MonoBehaviour
{
    public IEnumerator Transition(string targetScene, float fadeDuration, VideoClip transitionVideo = null)
    {
        DontDestroyOnLoad(gameObject);

        Canvas fadeCanvas = gameObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; 

        Image fadeImage = gameObject.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); 

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, elapsed / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black; 

        SceneManager.LoadScene(targetScene);
        
        yield return null;
        yield return null; 

        MovingAst[] players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = false;
        }

        if (transitionVideo != null)
        {
            GameObject videoObj = new GameObject("VideoUI");
            videoObj.transform.SetParent(fadeCanvas.transform, false);
            
            RawImage rawImage = videoObj.AddComponent<RawImage>();
            rawImage.rectTransform.anchorMin = Vector2.zero;
            rawImage.rectTransform.anchorMax = Vector2.one;
            rawImage.rectTransform.sizeDelta = Vector2.zero;
            rawImage.color = Color.clear; 

            VideoPlayer vp = gameObject.AddComponent<VideoPlayer>();
            vp.playOnAwake = false;
            vp.clip = transitionVideo;
            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.isLooping = false;

            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
            vp.targetTexture = rt;
            rawImage.texture = rt;

            vp.Prepare();
            while (!vp.isPrepared)
            {
                yield return null;
            }

            rawImage.color = Color.white; 
            vp.Play();

            yield return null;

            while (vp.isPlaying)
            {
                yield return null;
            }

            Destroy(videoObj);
            vp.targetTexture.Release();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = true;
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
            yield return null;
        }

        Destroy(gameObject);
    }
}