using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    public enum CharacterType { Center, LeftCharacter, RightCharacter }
    
    [Header("Player 1 Settings")]
    public CharacterType p1Choice = CharacterType.Center; 
    public bool p1Ready = false;
    private bool p1HasMoved = false; 

    public Image p1CardImage;      
    public Sprite p1IdleSprite;    
    public Sprite p1HoverSprite;   
    public Sprite p1ReadySprite;   

    [Header("Player 2 Settings")]
    public CharacterType p2Choice = CharacterType.Center; 
    public bool p2Ready = false;
    private bool p2HasMoved = false; 

    public Image p2CardImage;      
    public Sprite p2IdleSprite;    
    public Sprite p2HoverSprite;   
    public Sprite p2ReadySprite;   

    [Header("UI Positions (X 좌표)")]
    public float centerX = 0f;            
    public float leftCharacterX = -300f;  
    public float rightCharacterX = 300f;  
    public float moveSpeed = 15f;         

    [Header("Scene Transition")]
    public string nextSceneName = "GameScene"; 

    // ==========================================
    // [추가됨] 효과음 설정 변수
    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip readySound;
    // ==========================================

    void Start()
    {
        UpdateCardSprites();
    }

    void Update()
    {
        // 1. 카드 스무스 이동 (매 프레임)
        MoveCardsSmoothly();

        // ==========================================
        // 2. 플레이어 1 조작 (A, D, F)
        // ==========================================
        if (!p1Ready)
        {
            if (Input.GetKeyDown(KeyCode.A)) 
            {
                p1Choice = CharacterType.LeftCharacter;
                p1HasMoved = true; 
                UpdateCardSprites();
            }
            if (Input.GetKeyDown(KeyCode.D)) 
            {
                p1Choice = CharacterType.RightCharacter;
                p1HasMoved = true; 
                UpdateCardSprites();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!p1Ready)
            {
                if (p1Choice != CharacterType.Center) 
                {
                    bool isBlockedByP2 = (p1Choice == p2Choice && p2Ready);
                    
                    if (!isBlockedByP2) 
                    {
                        p1Ready = true;
                        PlayReadySound(); // [추가됨] 레디 성공 시 사운드 재생
                    }
                }
            }
            else
            {
                p1Ready = false;
            }
            UpdateCardSprites();
            CheckAllReady();
        }

        // ==========================================
        // 3. 플레이어 2 조작 (좌우 화살표, 우측 컨트롤)
        // ==========================================
        if (!p2Ready)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) 
            {
                p2Choice = CharacterType.LeftCharacter;
                p2HasMoved = true;
                UpdateCardSprites();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) 
            {
                p2Choice = CharacterType.RightCharacter;
                p2HasMoved = true;
                UpdateCardSprites();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            if (!p2Ready)
            {
                if (p2Choice != CharacterType.Center) 
                {
                    bool isBlockedByP1 = (p2Choice == p1Choice && p1Ready);

                    if (!isBlockedByP1) 
                    {
                        p2Ready = true;
                        PlayReadySound(); // [추가됨] 레디 성공 시 사운드 재생
                    }
                }
            }
            else
            {
                p2Ready = false;
            }
            UpdateCardSprites();
            CheckAllReady();
        }
    }

    void MoveCardsSmoothly()
    {
        if (p1CardImage != null)
        {
            float p1TargetX = centerX;
            if (p1Choice == CharacterType.LeftCharacter) p1TargetX = leftCharacterX;
            else if (p1Choice == CharacterType.RightCharacter) p1TargetX = rightCharacterX;

            Vector2 p1Pos = p1CardImage.rectTransform.anchoredPosition;
            p1Pos.x = Mathf.Lerp(p1Pos.x, p1TargetX, Time.deltaTime * moveSpeed);
            p1CardImage.rectTransform.anchoredPosition = p1Pos;
        }

        if (p2CardImage != null)
        {
            float p2TargetX = centerX;
            if (p2Choice == CharacterType.LeftCharacter) p2TargetX = leftCharacterX;
            else if (p2Choice == CharacterType.RightCharacter) p2TargetX = rightCharacterX;

            Vector2 p2Pos = p2CardImage.rectTransform.anchoredPosition;
            p2Pos.x = Mathf.Lerp(p2Pos.x, p2TargetX, Time.deltaTime * moveSpeed);
            p2CardImage.rectTransform.anchoredPosition = p2Pos;
        }
    }

    void UpdateCardSprites()
    {
        if (p1CardImage != null)
        {
            if (p1Ready) p1CardImage.sprite = p1ReadySprite;          
            else if (p1HasMoved) p1CardImage.sprite = p1HoverSprite;  
            else p1CardImage.sprite = p1IdleSprite;                   
        }

        if (p2CardImage != null)
        {
            if (p2Ready) p2CardImage.sprite = p2ReadySprite;          
            else if (p2HasMoved) p2CardImage.sprite = p2HoverSprite;  
            else p2CardImage.sprite = p2IdleSprite;                   
        }
    }

    void CheckAllReady()
    {
        if (p1Ready && p2Ready)
        {
            GameData.p1SelectedChar = (int)p1Choice;
            GameData.p2SelectedChar = (int)p2Choice;

            SceneManager.LoadScene(nextSceneName);
        }
    }

    // [추가됨] 효과음을 재생하는 헬퍼 함수
    void PlayReadySound()
    {
        if (sfxSource != null && readySound != null)
        {
            sfxSource.PlayOneShot(readySound);
        }
    }
}