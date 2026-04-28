using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCameraSetup : MonoBehaviour
{
    [Header("Camera Settings")]
    public Vector3 targetRotation = new Vector3(50f, 0f, 0f); // 이동 후 원하는 회전 각도

    void Start()
    {
        // 씬에 존재하는 메인 카메라를 찾습니다.
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            // 1. 카메라의 회전값 변경
            mainCam.transform.rotation = Quaternion.Euler(targetRotation);
            
            // 2. 카메라에 붙어있는 TitleCameraSetup 컴포넌트를 가져옵니다.
            TitleCameraSetup titleSetup = mainCam.GetComponent<TitleCameraSetup>();

            if (titleSetup != null && titleSetup.viewPositions.Length > 0)
            {
                // 현재 진행도(currentProgress)가 배열 범위를 벗어나지 않도록 클램프 처리합니다.
                int index = Mathf.Clamp(titleSetup.currentProgress, 0, titleSetup.viewPositions.Length - 1);
                
                // TitleCameraSetup 의 viewPositions 배열에서 해당 인덱스의 좌표를 가져와 적용합니다.
                mainCam.transform.position = titleSetup.viewPositions[index];
            }
        }
    }
}