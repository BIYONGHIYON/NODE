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

    private bool isTransitioning = false;

    void Start()
    {
        GameData.currentProgress = 0;

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

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                MoveToTitle();
            }
        }
    }

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

        transform.SetParent(null); 
        DontDestroyOnLoad(gameObject);

        StartCoroutine(FadeOutAndLoad(nextSceneName));
    }

    private IEnumerator FadeOutAndLoad(string targetSceneName)
    {
        Time.timeScale = 1f; 

        GameObject fadeObj = new GameObject("PersistentFadeCanvas");
        DontDestroyOnLoad(fadeObj);

        Canvas fadeCanvas = fadeObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; 

        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); 

        float fadeDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, elapsed / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black; 

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

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

        SceneManager.LoadScene(targetSceneName);

        yield return null;
        yield return null;

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

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
            yield return null;
        }

        Destroy(fadeObj);
        Destroy(gameObject);
    }
}