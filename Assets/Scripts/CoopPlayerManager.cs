using UnityEngine;

public class CoopPlayerManager : MonoBehaviour
{
    [Header("Characters")]
    public Transform character1; 
    public Transform character2; 

    [Header("Camera Tracking")]
    public float cameraSmoothTime = 0.2f;
    // 튜토리얼 씬처럼 특별한 연출이 끝난 후 추적을 시작해야 할 때 사용합니다.
    public bool isTracking = false; 

    private float xVelocity = 0.0f;
    private Camera mainCam;

    // Awake에서 미리 세팅해 두어야 다른 스크립트가 Start에서 참조할 때 꼬이지 않습니다.
    void Awake()
    {
        mainCam = Camera.main;
        
        SwapCharactersIfNecessary();
        AssignControls();
    }

    void Update()
    {
        // [핵심 추가] 씬 전환 등의 이유로 Awake에서 찾았던 카메라가 파괴되거나 놓쳐버렸다면, 
        // 다시 현재 씬의 진짜 메인 카메라를 찾아옵니다.
        if (mainCam == null || !mainCam.gameObject.activeInHierarchy)
        {
            mainCam = Camera.main;
        }

        // mainCam이 정상적으로 존재할 때만 추적을 실행합니다.
        if (isTracking && character1 != null && character2 != null && mainCam != null)
        {
            float centerX = (character1.position.x + character2.position.x) / 2f;
            Vector3 camPos = mainCam.transform.position;
            float targetX = Mathf.SmoothDamp(camPos.x, centerX, ref xVelocity, cameraSmoothTime);
            mainCam.transform.position = new Vector3(targetX, camPos.y, camPos.z);
        }
    }

    // 캐릭터 스왑 로직
    void SwapCharactersIfNecessary()
    {
        if (GameData.p1SelectedChar == 2)
        {
            if (character1 != null && character2 != null)
            {
                // 1. [추가됨] 화면에 보이는 실제 캐릭터들의 위치와 회전값을 서로 맞바꿉니다.
                Vector3 tempPos = character1.position;
                Quaternion tempRot = character1.rotation;

                character1.position = character2.position;
                character1.rotation = character2.rotation;

                character2.position = tempPos;
                character2.rotation = tempRot;

                // 2. 내부 변수(조작권 등)를 맞바꿉니다.
                Transform tempTransform = character1;
                character1 = character2;
                character2 = tempTransform;
            }
        }
    }

    // 조작 할당 로직
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
        if (rope != null) rope.ropeKey1 = KeyCode.F;
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