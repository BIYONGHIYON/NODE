using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform[] planetNodes; 
    public RectTransform selectorCursor; 
    
    [Header("Settings")]
    public float cursorMoveSpeed = 10f; 

    [Header("Scene Settings")]
    [Tooltip("씬 이름 대신 'Exit'라고 적으면 맵을 닫는 버튼으로 작동합니다.")]
    public string[] planetSceneNames; 
    
    [Header("System References")]
    public InteractionBox interactionBox; 

    private int currentIndex = 0;
    private bool isTransitioning = false; 

    void OnEnable()
    {
        isTransitioning = false;
        currentIndex = 0; // 찾지 못했을 때의 기본값은 0
        
        // 배열을 뒤져서 "Exit" (대소문자 구분 없이)가 적힌 인덱스를 찾습니다.
        for (int i = 0; i < planetSceneNames.Length; i++)
        {
            if (planetSceneNames[i].ToLower() == "exit")
            {
                currentIndex = i;
                break; // 찾았으면 반복문 종료
            }
        }

        if (planetNodes.Length > 0 && selectorCursor != null)
        {
            // 찾은 인덱스(나가기 버튼)의 위치로 커서를 즉시 이동시킵니다.
            selectorCursor.position = planetNodes[currentIndex].position;
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

        Vector3 targetPos = planetNodes[currentIndex].position;
        selectorCursor.position = Vector3.Lerp(selectorCursor.position, targetPos, Time.deltaTime * cursorMoveSpeed);
    }

    void SelectPlanet()
    {
        if (currentIndex < planetSceneNames.Length)
        {
            string targetName = planetSceneNames[currentIndex];

            // 1. 만약 배열에 적힌 이름이 "Exit" 이거나 "exit" 라면 맵 닫기 실행
            if (targetName == "Exit" || targetName == "exit")
            {
                CloseMap();
                return; // 씬 이동을 하지 않고 여기서 함수 종료
            }

            // 2. 일반 씬 이름이라면 해당 씬으로 이동
            if (!string.IsNullOrEmpty(targetName))
            {
                isTransitioning = true;
                SceneManager.LoadScene(targetName);
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