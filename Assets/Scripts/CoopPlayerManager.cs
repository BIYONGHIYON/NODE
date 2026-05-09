using UnityEngine;

public class CoopPlayerManager : MonoBehaviour
{
    [Header("Characters")]
    public Transform character1; 
    public Transform character2; 

    [Header("Camera Tracking")]
    public float cameraSmoothTime = 0.2f;
    public bool isTracking = false; 

    private float xVelocity = 0.0f;
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