using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class PlanetClearManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string tutorialSceneName = "TutorialScene"; 
    
    [Header("Fade Settings")]
    public float fadeDuration = 1f; 

    private Camera persistentCam;
    private bool isTransitioning = false;

    void Start()
    {
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.scene.name == "DontDestroyOnLoad")
            {
                persistentCam = cam;
                persistentCam.gameObject.SetActive(false); 
                break;
            }
        }
    }

    public void ReturnToTutorial()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        // 1. 파괴되지 않는 검은 화면 UI 캔버스 생성
        GameObject fadeObj = new GameObject("PersistentFadeCanvas");
        DontDestroyOnLoad(fadeObj);

        Canvas fadeCanvas = fadeObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; 

        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); 

        // 2. 페이드 아웃
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, elapsed / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black; 

        // 3. 기존 카메라 켜고 튜토리얼 씬 로드
        if (persistentCam != null) persistentCam.gameObject.SetActive(true);
        SceneManager.LoadScene(tutorialSceneName);

        // 4. 새 씬의 오브젝트들이 모두 로드될 때까지 잠시 대기
        yield return null;
        yield return null;

        // 5. 튜토리얼 씬의 캐릭터들을 찾아 조작 일시정지
        MovingAst[] players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = false;
        }

        // 6. TutorialCameraSetup의 백그라운드 카메라 연출(약 3.5초)이 끝날 때까지 대기
        yield return new WaitForSeconds(3.6f);

        // 7. 대기하는 동안 캐릭터 찾아서 조작을 재개
        players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = true;
        }

        // 8. 페이드 인
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
            yield return null;
        }

        // 9. 연출이 끝났으니 임시 캔버스 파괴
        Destroy(fadeObj);

        // 10. SaturnManager 파괴
        Destroy(gameObject);
    }
}