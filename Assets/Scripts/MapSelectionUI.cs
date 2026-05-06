using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections; // 코루틴을 위해 추가됨

public class MapSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform[] planetNodes; 
    public RectTransform selectorCursor; 
    
    [Header("Settings")]
    public float cursorMoveSpeed = 10f; 
    public Vector3 exitCursorOffset = new Vector3(0f, -15f, 0f); 

    [Header("Fade Settings")]
    public float fadeDuration = 1f; // 페이드 연출 시간

    [Header("Scene Settings")]
    public string[] planetSceneNames; 
    
    [Header("System References")]
    public InteractionBox interactionBox; 

    private int currentIndex = 0;
    private bool isTransitioning = false; 

    void OnEnable()
    {
        isTransitioning = false;
        currentIndex = 0; 
        
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

            if (!string.IsNullOrEmpty(targetName))
            {
                isTransitioning = true;
                
                // [핵심 변경됨] 씬을 즉시 로드하지 않고, 파괴되지 않는 헬퍼 오브젝트를 만들어 페이드 연출을 맡깁니다.
                GameObject helperObj = new GameObject("TransitionHelper");
                SceneTransitionHelper helper = helperObj.AddComponent<SceneTransitionHelper>();
                helper.StartCoroutine(helper.Transition(targetName, fadeDuration));
            }
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

// ====================================================================================
// [추가됨] 씬이 넘어가도 파괴되지 않고 페이드 아웃 -> 씬 로드 -> 페이드 인을 책임지는 헬퍼 클래스
// ====================================================================================
public class SceneTransitionHelper : MonoBehaviour
{
    public IEnumerator Transition(string targetScene, float fadeDuration)
    {
        // 헬퍼 자신을 파괴 불가 상태로 만듭니다.
        DontDestroyOnLoad(gameObject);

        // 1. 임시 검은 화면 캔버스 생성
        Canvas fadeCanvas = gameObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; 

        Image fadeImage = gameObject.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); 

        // 2. 화면이 서서히 까매짐 (Fade Out)
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, elapsed / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black; 

        // 3. 씬 로드
        SceneManager.LoadScene(targetScene);
        
        // 씬이 완전히 불러와질 때까지 프레임 대기
        yield return null;
        yield return null; 

        // 4. 새 씬에 있는 플레이어들의 조작을 끕니다.
        MovingAst[] players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = false;
        }

        // 5. 튜토리얼 씬과 달리 행성 씬은 카메라 이동 대기(3.6초)가 필요 없으므로, 
        // 씬 로드 직후 발생할 수 있는 렉이 진정되도록 0.5초만 짧게 대기합니다.
        yield return new WaitForSeconds(0.5f);

        // 6. 플레이어 조작 복구
        players = FindObjectsOfType<MovingAst>();
        foreach(var p in players) 
        {
            if (p != null) p.enabled = true;
        }

        // 7. 화면이 다시 밝아짐 (Fade In)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
            yield return null;
        }

        // 8. 연출이 모두 끝났으므로 헬퍼 스스로 삭제
        Destroy(gameObject);
    }
}