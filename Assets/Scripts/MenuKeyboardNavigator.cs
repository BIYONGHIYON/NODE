using UnityEngine;
using UnityEngine.UI;

public class MenuKeyboardNavigator : MonoBehaviour
{
    [Header("메뉴 항목 (위에서 아래 순서대로 드래그)")]
    public Selectable[] menuItems;

    [Header("선택 표시용 커서 (UI Image)")]
    public RectTransform cursorRect; 
    
    [Tooltip("바의 두께 (너비)")]
    public float cursorWidth = 5f; 
    
    [Tooltip("메뉴 항목 오른쪽 끝에서 얼마나 떨어질지 간격")]
    public float cursorSpacing = 5f; 

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
        cursorRect.pivot = new Vector2(0f, 0.5f); 

        cursorRect.localScale = targetRect.localScale; 

        cursorRect.sizeDelta = new Vector2(cursorWidth, targetRect.rect.height);

        Vector3 rightEdgeLocalPos = new Vector3(targetRect.rect.xMax + cursorSpacing, targetRect.rect.center.y, 0f);
        
        cursorRect.position = targetRect.TransformPoint(rightEdgeLocalPos);
    }
}