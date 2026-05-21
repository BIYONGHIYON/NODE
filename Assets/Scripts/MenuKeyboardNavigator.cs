using UnityEngine;
using UnityEngine.UI;

public class MenuKeyboardNavigator : MonoBehaviour
{
    [Header("메뉴 항목 (위에서 아래 순서대로 드래그)")]
    public Selectable[] menuItems;

    // ==========================================
    [Header("선택 표시용 커서 (UI Image)")]
    public RectTransform cursorRect; 
    
    [Tooltip("바의 두께 (너비)")]
    public float cursorWidth = 5f; 
    
    [Tooltip("메뉴 항목 오른쪽 끝에서 얼마나 떨어질지 간격")]
    public float cursorSpacing = 5f; 
    // ==========================================

    [Header("슬라이더 조절 속도")]
    public float sliderAdjustSpeed = 1.5f;

    private int currentIndex = 0;

    void OnEnable()
    {
        currentIndex = 0;
        UpdateCursorPosition();
    }

    void Update()
    {
        if (menuItems == null || menuItems.Length == 0) return;

        // 1. 위아래 이동
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = menuItems.Length - 1; 
            UpdateCursorPosition();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex++;
            if (currentIndex >= menuItems.Length) currentIndex = 0; 
            UpdateCursorPosition();
        }

        // 2. 버튼 클릭
        if (Input.GetKeyDown(KeyCode.F) || 
            Input.GetKeyDown(KeyCode.RightAlt) || 
            Input.GetKeyDown(KeyCode.RightControl) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            Selectable currentItem = menuItems[currentIndex];
            if (currentItem is Button)
            {
                ((Button)currentItem).onClick.Invoke();
            }
        }

        // 3. 슬라이더 값 조절
        Selectable currentSliderItem = menuItems[currentIndex];
        if (currentSliderItem is Slider)
        {
            Slider slider = (Slider)currentSliderItem;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                slider.value -= sliderAdjustSpeed * Time.unscaledDeltaTime;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                slider.value += sliderAdjustSpeed * Time.unscaledDeltaTime;
            }
        }
    }

    void UpdateCursorPosition()
    {
        if (cursorRect == null || menuItems.Length == 0 || menuItems[currentIndex] == null) return;

        RectTransform targetRect = menuItems[currentIndex].GetComponent<RectTransform>();
        
        cursorRect.SetParent(targetRect.parent, false);
        cursorRect.SetAsLastSibling();

        // 계산을 아주 쉽게 하기 위해 커서의 피벗(기준점)을 '왼쪽 중앙'으로 맞춥니다.
        cursorRect.anchorMin = new Vector2(0.5f, 0.5f);
        cursorRect.anchorMax = new Vector2(0.5f, 0.5f);
        cursorRect.pivot = new Vector2(0f, 0.5f); 

        cursorRect.localScale = targetRect.localScale; 

        // 1. 크기 변경: 높이는 타겟 메뉴와 똑같이 맞추고, 두께는 설정한 고정값(cursorWidth)으로 설정합니다.
        cursorRect.sizeDelta = new Vector2(cursorWidth, targetRect.rect.height);

        // 2. 위치 변경: 타겟의 로컬 좌표 기준으로 '오른쪽 끝(xMax)'에서 '간격(cursorSpacing)'만큼 떨어진 곳의 월드 좌표를 구합니다.
        Vector3 rightEdgeLocalPos = new Vector3(targetRect.rect.xMax + cursorSpacing, targetRect.rect.center.y, 0f);
        
        // 구한 좌표를 적용합니다.
        cursorRect.position = targetRect.TransformPoint(rightEdgeLocalPos);
    }
}