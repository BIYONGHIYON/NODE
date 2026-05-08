using UnityEngine;
using UnityEngine.UI;

public class MenuKeyboardNavigator : MonoBehaviour
{
    [Header("메뉴 항목 (위에서 아래 순서대로 드래그)")]
    public Selectable[] menuItems;

    // ==========================================
    // [추가됨] 선택된 메뉴를 따라다닐 커서(테두리) UI
    [Header("선택 표시용 커서 (UI Image)")]
    public RectTransform cursorRect; 
    public Vector2 cursorPadding = new Vector2(20f, 20f); // 테두리를 메뉴보다 얼마나 더 크게 할지 여백
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
            Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || 
            Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
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

    // [수정됨] 커서가 선택된 UI 요소의 위치와 크기를 그대로 따라가게 만듭니다.
    // [수정됨] 스케일(Scale) 뻥튀기 버그와 피벗 틀어짐을 완벽하게 해결한 버전
    void UpdateCursorPosition()
    {
        if (cursorRect == null || menuItems.Length == 0 || menuItems[currentIndex] == null) return;

        RectTransform targetRect = menuItems[currentIndex].GetComponent<RectTransform>();
        
        // 1. [핵심 해결] 두 번째 값으로 'false'를 넣어, 부모를 옮길 때 스케일이 제멋대로 커지는 것을 막습니다!
        cursorRect.SetParent(targetRect.parent, false);
        cursorRect.SetAsLastSibling();

        // 2. 앵커는 정중앙으로 강제 고정
        cursorRect.anchorMin = new Vector2(0.5f, 0.5f);
        cursorRect.anchorMax = new Vector2(0.5f, 0.5f);

        // 3. [핵심 해결] 대상 버튼의 피벗(기준점)과 스케일(크기 배율)을 그대로 훔쳐옵니다!
        cursorRect.pivot = targetRect.pivot; 
        cursorRect.localScale = targetRect.localScale; 

        // 4. 이제 안심하고 위치와 기본 픽셀 크기를 적용합니다.
        cursorRect.position = targetRect.position;
        cursorRect.sizeDelta = targetRect.rect.size + cursorPadding; 
    }
}