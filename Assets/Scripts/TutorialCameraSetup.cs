using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCameraSetup : MonoBehaviour
{
    [Header("Camera Settings")]
    public Vector3 targetRotation = new Vector3(15f, 70f, 0f);
    public float cameraSmoothTime = 0.2f; 

    [Header("Spaceship Settings")]
    public Transform spaceshipObject;
    public Vector3 localPositionOffset = new Vector3(0f, 0f, 10f); 
    public Vector3 localRotationOffset = Vector3.zero;

    [Header("Character Tracking & Placement")]
    // 기존처럼 인스펙터에서 씬에 있는 캐릭터를 직접 끌어다 넣으시면 됩니다!
    public Transform character1; 
    public Transform character2; 
    
    [Space(10)]
    [Header("Character 1 Settings (왼쪽 자리)")]
    public Vector3 char1LocalOffset = new Vector3(-1.5f, 0f, 15f); 
    public Vector3 char1LocalRotation = Vector3.zero;
    public Vector3 char1LocalScale = Vector3.one; 
    
    [Space(5)]
    [Header("Character 2 Settings (오른쪽 자리)")]
    public Vector3 char2LocalOffset = new Vector3(1.5f, 0f, 15f); 
    public Vector3 char2LocalRotation = Vector3.zero;
    public Vector3 char2LocalScale = Vector3.one; 

    private bool isTrackingStarted = false; 
    private float xVelocity = 0.0f;

    void Start()
    {
        // [핵심 추가] 게임 시작 직후, P1이 2번 캐릭터를 골랐다면 위치를 바꾸기 위해 변수를 스왑합니다.
        // 이렇게 하면 이후 로직에서 character1은 무조건 'P1이 조작하는 왼쪽 캐릭터'가 됩니다.
        if (GameData.p1SelectedChar == 2)
        {
            Transform temp = character1;
            character1 = character2;
            character2 = temp;
        }

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            TitleCameraSetup titleSetup = mainCam.GetComponent<TitleCameraSetup>();
            if (titleSetup != null && titleSetup.viewPositions.Length > 0)
            {
                int index = Mathf.Clamp(titleSetup.currentProgress, 0, titleSetup.viewPositions.Length - 1);
                Vector3 finalCamPos = titleSetup.viewPositions[index];
                Quaternion finalCamRot = Quaternion.Euler(targetRotation);

                Matrix4x4 camMatrix = Matrix4x4.TRS(finalCamPos, finalCamRot, Vector3.one);

                if (spaceshipObject != null)
                {
                    spaceshipObject.position = camMatrix.MultiplyPoint3x4(localPositionOffset);
                    spaceshipObject.rotation = finalCamRot * Quaternion.Euler(localRotationOffset);
                }

                // 스왑된 변수를 바탕으로 배치를 진행하므로 자동으로 P1 캐릭터가 왼쪽(-1.5f)에 배치됩니다.
                PlaceCharactersRelativeToCamera(camMatrix, finalCamRot);

                // 스왑이 완료되었으므로 무조건 character1에게 P1 조작을 주면 됩니다.
                AssignControls();

                StartCoroutine(SmoothTransition(mainCam.transform, finalCamPos, targetRotation));
            }
        }
    }

    void Update()
    {
        if (isTrackingStarted && character1 != null && character2 != null)
        {
            float centerX = (character1.position.x + character2.position.x) / 2f;
            Vector3 camPos = Camera.main.transform.position;
            float targetX = Mathf.SmoothDamp(camPos.x, centerX, ref xVelocity, cameraSmoothTime);
            Camera.main.transform.position = new Vector3(targetX, camPos.y, camPos.z);
        }
    }

    void PlaceCharactersRelativeToCamera(Matrix4x4 camMatrix, Quaternion camRot)
    {
        if (character1 != null)
        {
            character1.position = camMatrix.MultiplyPoint3x4(char1LocalOffset);
            character1.rotation = camRot * Quaternion.Euler(char1LocalRotation);
            character1.localScale = char1LocalScale;
        }

        if (character2 != null)
        {
            character2.position = camMatrix.MultiplyPoint3x4(char2LocalOffset);
            character2.rotation = camRot * Quaternion.Euler(char2LocalRotation);
            character2.localScale = char2LocalScale;
        }
    }

    IEnumerator SmoothTransition(Transform camTransform, Vector3 endPos, Vector3 endRotEuler)
    {
        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;
        
        float targetX = -90f;
        TitleCameraSetup titleSetup = camTransform.GetComponent<TitleCameraSetup>();
        if (titleSetup != null) targetX = titleSetup.GetPhase1XRotation();

        Vector3 currentEuler = startRot.eulerAngles;
        Quaternion phase1Rot = Quaternion.Euler(targetX, currentEuler.y, currentEuler.z);
        
        Quaternion finalRot = Quaternion.Euler(endRotEuler);

        float elapsed = 0f;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            camTransform.rotation = Quaternion.Slerp(startRot, phase1Rot, Mathf.SmoothStep(0f, 1f, elapsed / 1.0f));
            yield return null;
        }
        camTransform.rotation = phase1Rot;

        elapsed = 0f;
        while (elapsed < 2.0f)
        {
            elapsed += Time.deltaTime;
            camTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, elapsed / 2.0f));
            yield return null;
        }
        camTransform.position = endPos;

        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            camTransform.rotation = Quaternion.Slerp(phase1Rot, finalRot, Mathf.SmoothStep(0f, 1f, elapsed / 0.5f));
            yield return null;
        }
        camTransform.rotation = finalRot;

        isTrackingStarted = true;
    }

    // [변경됨] 복잡한 조건문 없이 직관적으로 조작을 할당합니다.
    void AssignControls()
    {
        if (character1 == null || character2 == null) return;

        MovingAst leftMove = character1.GetComponent<MovingAst>();
        RopeAction leftRope = character1.GetComponent<RopeAction>();

        MovingAst rightMove = character2.GetComponent<MovingAst>();
        RopeAction rightRope = character2.GetComponent<RopeAction>();

        // character1은 이제 무조건 P1의 캐릭터이자 왼쪽 자리이므로 P1 컨트롤을 부여합니다.
        ApplyP1Controls(leftMove, leftRope);

        // character2는 무조건 P2의 캐릭터이자 오른쪽 자리이므로 P2 컨트롤을 부여합니다.
        ApplyP2Controls(rightMove, rightRope); 
    }

    // P1 (WASD + F) 키 세팅용 헬퍼 함수
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

    // P2 (방향키 + RightControl) 키 세팅용 헬퍼 함수
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