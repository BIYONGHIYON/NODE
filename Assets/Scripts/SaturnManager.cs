using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class SaturnManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string tutorialSceneName = "TutorialScene"; 
    
    [Header("Fade Settings")]
    public float fadeDuration = 1f; 

    private Camera persistentCam;
    private bool isTransitioning = false; // [추가됨] 버튼 중복 클릭 방지

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
        if (isTransitioning) return; // 이미 전환 중이면 무시
        isTransitioning = true;

        // [핵심 해결 방법]
        // 씬이 넘어가더라도 이 스크립트의 코루틴이 멈추지 않도록 매니저 자신을 파괴 방지 처리합니다.
        transform.SetParent(null); // 최상위 오브젝트로 만들어야 DontDestroyOnLoad가 작동함
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

        // 2. 페이드 아웃 (화면 까매짐)
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

        // 5. 튜토리얼 씬의 캐릭터들을 찾아 조작을 끕니다.
        MovingAst[] players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = false;
        }

        // 6. TutorialCameraSetup의 백그라운드 카메라 연출(약 3.5초)이 끝날 때까지 넉넉히 대기
        yield return new WaitForSeconds(3.6f);

        // 7. 대기하는 동안 캐릭터 참조가 끊겼을 수 있으니 다시 찾아서 조작을 켭니다.
        players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = true;
        }

        // 8. 페이드 인 (화면 밝아짐)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
            yield return null;
        }

        // 9. 연출이 끝났으니 임시 캔버스 파괴
        Destroy(fadeObj);

        // 10. [핵심] 코루틴을 끝까지 돌려준 SaturnManager 자신도 이제 쓸모가 다했으므로 스스로 파괴
        Destroy(gameObject);
    }
}