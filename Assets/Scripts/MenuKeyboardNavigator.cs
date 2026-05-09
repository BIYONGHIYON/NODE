using UnityEngine;
using UnityEngine.UI;

public class MenuKeyboardNavigator : MonoBehaviour
{
    [Header("메뉴 항목 (위에서 아래 순서대로 드래그)")]
    public Selectable[] menuItems;

    // ==========================================
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

        cursorRect.anchorMin = new Vector2(0.5f, 0.5f);
        cursorRect.anchorMax = new Vector2(0.5f, 0.5f);

        cursorRect.pivot = targetRect.pivot; 
        cursorRect.localScale = targetRect.localScale; 

        cursorRect.position = targetRect.position;
        cursorRect.sizeDelta = targetRect.rect.size + cursorPadding; 
    }
}