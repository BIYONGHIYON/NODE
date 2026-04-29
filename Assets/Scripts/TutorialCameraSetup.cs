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
    public Transform character1;
    public Transform character2;
    
    [Space(10)]
    [Header("Character 1 Settings")]
    public Vector3 char1LocalOffset = new Vector3(-1.5f, 0f, 15f); // 카메라 기준 오프셋
    public Vector3 char1LocalRotation = Vector3.zero;
    public Vector3 char1LocalScale = Vector3.one; 
    
    [Space(5)]
    [Header("Character 2 Settings")]
    public Vector3 char2LocalOffset = new Vector3(1.5f, 0f, 15f); // 카메라 기준 오프셋
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

                // 카메라의 최종 위치와 회전을 기준으로 하는 변환 행렬 생성
                Matrix4x4 camMatrix = Matrix4x4.TRS(finalCamPos, finalCamRot, Vector3.one);

                if (spaceshipObject != null)
                {
                    // 1. 우주선 배치 (카메라 좌표계 기준)
                    spaceshipObject.position = camMatrix.MultiplyPoint3x4(localPositionOffset);
                    spaceshipObject.rotation = finalCamRot * Quaternion.Euler(localRotationOffset);
                }

                // 2. 캐릭터 배치 (우주선이 아닌 카메라 좌표계를 기준으로 직접 배치)
                PlaceCharactersRelativeToCamera(camMatrix, finalCamRot);

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

    // 우주선의 Transform 대신 카메라의 Matrix와 Rotation을 인자로 받습니다.
    void PlaceCharactersRelativeToCamera(Matrix4x4 camMatrix, Quaternion camRot)
    {
        if (character1 != null)
        {
            // 위치: 카메라 좌표계 기준 오프셋 적용
            character1.position = camMatrix.MultiplyPoint3x4(char1LocalOffset);

            // 회전: 카메라의 회전값에 캐릭터의 로컬 오프셋 각도를 결합
            // 이 방식은 우주선이 회전 기준이 아니므로 짐벌락 현상이 발생하지 않습니다.
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
}