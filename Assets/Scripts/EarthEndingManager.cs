using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class EarthEndingManager : MonoBehaviour
{
    [Header("비디오 설정")]
    [Tooltip("엔딩 영상을 재생할 VideoPlayer를 연결하세요.")]
    public VideoPlayer videoPlayer;

    [Header("씬 이동 설정")]
    public string nextSceneName = "TitleScene";

    private bool isTransitioning = false; // 중복 실행 방지용

    void Start()
    {
        // 1. 지구에 도착했으니 진행도를 0으로 완벽하게 초기화합니다.
        GameData.currentProgress = 0;

        // 2. 영상 재생 시작 및 종료 이벤트 연결
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play();
        }
        else
        {
            MoveToTitle();
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        // 엔딩 크레딧이나 영상을 스킵하고 싶을 때 Esc나 Enter
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                MoveToTitle();
            }
        }
    }

    // 영상 재생이 끝까지 도달했을 때 자동으로 호출되는 함수
    void OnVideoEnd(VideoPlayer vp)
    {
        if (isTransitioning) return;
        MoveToTitle();
    }

    void MoveToTitle()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        // 씬이 넘어가도 매니저가 파괴되지 않도록 최상단으로 분리 후 보호
        transform.SetParent(null); 
        DontDestroyOnLoad(gameObject);

        // 메뉴 매니저와 동일한 페이드 아웃 코루틴 실행
        StartCoroutine(FadeOutAndLoad(nextSceneName));
    }

    // ==========================================================
    // [추가됨] GameMenuManager의 TitleScene 이동 로직 완벽 이식
    // ==========================================================
    private IEnumerator FadeOutAndLoad(string targetSceneName)
    {
        Time.timeScale = 1f; 

        // 1. 페이드 효과를 담당할 영구 캔버스 생성
        GameObject fadeObj = new GameObject("PersistentFadeCanvas");
        DontDestroyOnLoad(fadeObj);

        Canvas fadeCanvas = fadeObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; 

        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); 

        // 2. 페이드 아웃 (투명 -> 검은색)
        float fadeDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, elapsed / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black; 

        // 3. 화면이 완전히 까매졌을 때 영상을 끕니다 (화면 튀는 현상 방지)
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        // 4. DontDestroyOnLoad 카메라 추적
        Camera persistentCam = null;
        Camera[] allCameras = FindObjectsOfType<Camera>(true); 
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.scene.name == "DontDestroyOnLoad")
            {
                persistentCam = cam;
                persistentCam.gameObject.SetActive(true); 
                break;
            }
        }

        // 5. 씬 로드 및 유니티 프레임 대기
        SceneManager.LoadScene(targetSceneName);

        yield return null;
        yield return null;

        // 6. TitleScene 진입 시 중복 카메라 파괴 로직
        if (targetSceneName == "TitleScene" && persistentCam != null)
        {
            Camera[] currentCameras = FindObjectsOfType<Camera>();
            foreach(Camera cam in currentCameras)
            {
                if (cam != persistentCam && cam.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    Destroy(cam.gameObject);
                }
            }
        }

        // 7. 페이드 인 (검은색 -> 투명)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
            yield return null;
        }

        // 8. 연출이 끝났으니 생성했던 캔버스와 매니저 자신을 파괴
        Destroy(fadeObj);
        Destroy(gameObject);
    }
}