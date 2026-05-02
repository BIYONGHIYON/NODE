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
    public Transform character1; // 무조건 씬의 '왼쪽' 캐릭터 할당
    public Transform character2; // 무조건 씬의 '오른쪽' 캐릭터 할당
    
    [Space(10)]
    [Header("Character 1 Settings")]
    public Vector3 char1LocalOffset = new Vector3(-1.5f, 0f, 15f); 
    public Vector3 char1LocalRotation = Vector3.zero;
    public Vector3 char1LocalScale = Vector3.one; 
    
    [Space(5)]
    [Header("Character 2 Settings")]
    public Vector3 char2LocalOffset = new Vector3(1.5f, 0f, 15f); 
    public Vector3 char2LocalRotation = Vector3.zero;
    public Vector3 char2LocalScale = Vector3.one; 

    private bool isTrackingStarted = false; 
    private float xVelocity = 0.0f;

    void Start()
    {
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

                // 기존 오프셋 배치 로직 그대로 사용
                PlaceCharactersRelativeToCamera(camMatrix, finalCamRot);

                // [핵심] 캐릭터 선택 결과에 따라 조작키를 씬에 있는 캐릭터에게 나눠줍니다.
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

    // [변경됨] 씬에 이미 존재하는 캐릭터들의 컴포넌트를 가져와서 키만 세팅합니다.
    void AssignControls()
    {
        if (character1 == null || character2 == null) return;

        MovingAst leftMove = character1.GetComponent<MovingAst>();
        RopeAction leftRope = character1.GetComponent<RopeAction>();

        MovingAst rightMove = character2.GetComponent<MovingAst>();
        RopeAction rightRope = character2.GetComponent<RopeAction>();

        // [수정된 부분] 로그에 1이 찍힐 때 왼쪽 캐릭터를 주도록 조건을 바꿉니다.
        if (GameData.p1SelectedChar == 1) 
        {
            ApplyP1Controls(leftMove, leftRope);   // 왼쪽 -> P1(WASD)
            ApplyP2Controls(rightMove, rightRope); // 오른쪽 -> P2(방향키)
        }
        else // 결과가 2(오른쪽)일 때
        {
            ApplyP1Controls(rightMove, rightRope); // 오른쪽 -> P1(WASD)
            ApplyP2Controls(leftMove, leftRope);   // 왼쪽 -> P2(방향키)
        }
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
        if (rope != null) rope.ropeKey = KeyCode.F;
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
        if (rope != null) rope.ropeKey = KeyCode.RightControl;
    }
}