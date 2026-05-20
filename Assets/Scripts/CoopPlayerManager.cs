using UnityEngine;
using UnityEngine.SceneManagement; 

public class CoopPlayerManager : MonoBehaviour
{
    [Header("Characters")]
    public Transform character1; 
    public Transform character2; 

    [Header("Camera Tracking (X & Y Axis)")]
    public float cameraSmoothTime = 0.2f;
    public bool isTracking = false; 
    
    // ==========================================
    // [추가됨] Y축 카메라 높이 조절용 오프셋
    public float trackingYOffset = 0f; // 카메라가 캐릭터 중앙보다 얼마나 더 위(또는 아래)를 비출지 정합니다.
    // ==========================================

    [Header("Camera Zoom (Z Axis)")]
    public float minDistance = 5f;    
    public float maxDistance = 20f;   
    public float defaultZ = -15f;     
    public float zoomedOutZ = -30f;   
    public float zoomSmoothTime = 0.5f; 

    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f; // [추가됨] Y축 스무스 이동용 변수
    private float zVelocity = 0.0f; 
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        
        SwapCharactersIfNecessary();
        AssignControls();
    }

    void Update()
    {
        if (mainCam == null || !mainCam.gameObject.activeInHierarchy)
        {
            mainCam = Camera.main;
        }

        if (isTracking && character1 != null && character2 != null && mainCam != null)
        {
            // 1. X축 중앙 따라가기 (공통 로직 - 항상 실행)
            float centerX = (character1.position.x + character2.position.x) / 2f;
            Vector3 camPos = mainCam.transform.position;
            float targetX = Mathf.SmoothDamp(camPos.x, centerX, ref xVelocity, cameraSmoothTime);

            // 튜토리얼 씬일 경우를 대비해 Y와 Z의 기본값은 현재 카메라 위치로 유지합니다.
            float targetY = camPos.y; 
            float targetZ = camPos.z; 

            // 2. Y축 & Z축 다이내믹 이동 (TutorialScene이 아닐 때만 실행)
            if (SceneManager.GetActiveScene().name != "TutorialScene")
            {
                // [Y축 로직] 두 캐릭터의 Y축 중앙값을 구하고 오프셋을 더해줍니다.
                float centerY = (character1.position.y + character2.position.y) / 2f;
                targetY = Mathf.SmoothDamp(camPos.y, centerY + trackingYOffset, ref yVelocity, cameraSmoothTime);

                // [Z축 로직] 두 캐릭터 사이의 거리에 따른 줌아웃
                float distance = Vector3.Distance(character1.position, character2.position);
                float zoomPercent = Mathf.InverseLerp(minDistance, maxDistance, distance);
                float desiredZ = Mathf.Lerp(defaultZ, zoomedOutZ, zoomPercent);
                targetZ = Mathf.SmoothDamp(camPos.z, desiredZ, ref zVelocity, zoomSmoothTime);
            }

            // 3. 최종 위치 적용 (X, Y, Z 모두 병합)
            mainCam.transform.position = new Vector3(targetX, targetY, targetZ);
        }
    }

    // 캐릭터 스왑 로직
    void SwapCharactersIfNecessary()
    {
        if (GameData.p1SelectedChar == 2)
        {
            if (character1 != null && character2 != null)
            {
                Vector3 tempPos = character1.position;
                Quaternion tempRot = character1.rotation;

                character1.position = character2.position;
                character1.rotation = character2.rotation;

                character2.position = tempPos;
                character2.rotation = tempRot;

                Transform tempTransform = character1;
                character1 = character2;
                character2 = tempTransform;
            }
        }
    }

    void AssignControls()
    {
        if (character1 == null || character2 == null) return;

        MovingAst leftMove = character1.GetComponent<MovingAst>();
        RopeAction leftRope = character1.GetComponent<RopeAction>();

        MovingAst rightMove = character2.GetComponent<MovingAst>();
        RopeAction rightRope = character2.GetComponent<RopeAction>();

        ApplyP1Controls(leftMove, leftRope);
        ApplyP2Controls(rightMove, rightRope); 
    }

    void ApplyP1Controls(MovingAst move, RopeAction rope)
    {
        if (move != null)
        {
            move.upKey = KeyCode.W;
            move.downKey = KeyCode.S;
            move.leftKey = KeyCode.A;
            move.rightKey = KeyCode.D;
        }
        if (rope != null) 
        {
            rope.ropeKey1 = KeyCode.F;
            
            // =========================================================
            // [핵심 해결] 프리팹에 남아있던 P2의 키(RightAlt)를 확실하게 지워줍니다!
            rope.ropeKey2 = KeyCode.None; 
            // =========================================================
        }
    }

    void ApplyP2Controls(MovingAst move, RopeAction rope)
    {
        if (move != null)
        {
            move.upKey = KeyCode.UpArrow;
            move.downKey = KeyCode.DownArrow;
            move.leftKey = KeyCode.LeftArrow;
            move.rightKey = KeyCode.RightArrow;
        }
        if (rope != null) {
            rope.ropeKey1 = KeyCode.RightControl;
            rope.ropeKey2 = KeyCode.RightAlt;
        }
    }
}