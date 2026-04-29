using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCameraSetup : MonoBehaviour
{
    [Header("Camera Settings")]
    public Vector3 targetRotation = new Vector3(15f, 70f, 0f); // 카메라가 도착할 목표 회전
    public float transitionDuration = 2.0f;

    [Header("Spaceship Settings")]
    public Transform spaceshipObject;
    // 이제 이 오프셋은 '카메라 기준 로컬 좌표'처럼 작동합니다.
    public Vector3 localPositionOffset = new Vector3(0f, 0f, 10f); 
    public Vector3 localRotationOffset = Vector3.zero;

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

                // --- [로컬 좌표계 방식 계산] ---
                if (spaceshipObject != null)
                {
                    // 1. 행렬(Matrix)을 생성합니다. (카메라가 도착했을 때의 가상의 틀)
                    // 이 행렬은 최종 위치와 회전을 기준으로 한 '기준점'이 됩니다.
                    Matrix4x4 targetMatrix = Matrix4x4.TRS(finalCamPos, finalCamRot, Vector3.one);

                    // 2. 행렬을 사용하여 로컬 오프셋을 월드 좌표로 변환합니다.
                    spaceshipObject.position = targetMatrix.MultiplyPoint3x4(localPositionOffset);
                    
                    // 3. 회전 또한 로컬 기준으로 더해줍니다.
                    spaceshipObject.rotation = finalCamRot * Quaternion.Euler(localRotationOffset);
                }

                // 카메라 이동 시작
                StartCoroutine(SmoothTransition(mainCam.transform, finalCamPos, targetRotation));
            }
        }
    }

    IEnumerator SmoothTransition(Transform camTransform, Vector3 endPos, Vector3 endRotEuler)
    {
        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;
        
        // --- [수정된 부분] TitleCameraSetup에서 현재 진행도에 맞는 X각도를 가져옴 ---
        float targetX = -90f; // 기본값
        TitleCameraSetup titleSetup = camTransform.GetComponent<TitleCameraSetup>();
        if (titleSetup != null)
        {
            targetX = titleSetup.GetPhase1XRotation();
        }

        // 1단계 목표: 가져온 targetX 사용 (Y, Z는 0으로 고정하거나 상황에 맞게 조절)
        Quaternion phase1Rot = Quaternion.Euler(targetX, 0f, 0f);
        Quaternion finalRot = Quaternion.Euler(endRotEuler);

        // --- [1단계] 1초 동안 가변적인 X각도로 회전 ---
        float elapsed = 0f;
        float duration1 = 1.0f;
        while (elapsed < duration1)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration1);
            camTransform.rotation = Quaternion.Slerp(startRot, phase1Rot, t);
            yield return null;
        }
        camTransform.rotation = phase1Rot;

        // --- [2단계] 2초 동안 포지션 이동 (동일) ---
        elapsed = 0f;
        float duration2 = 2.0f;
        while (elapsed < duration2)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration2);
            camTransform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        camTransform.position = endPos;

        // --- [3단계] 0.5초 동안 최종 사용자 각도로 회전 (동일) ---
        elapsed = 0f;
        float duration3 = 0.5f;
        while (elapsed < duration3)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration3);
            camTransform.rotation = Quaternion.Slerp(phase1Rot, finalRot, t);
            yield return null;
        }
        camTransform.rotation = finalRot;
    }
}