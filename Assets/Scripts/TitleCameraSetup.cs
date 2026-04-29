using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; // [추가됨] 비디오 제어를 위해 필요합니다.

public class TitleCameraSetup : MonoBehaviour
{
    [Header("Camera Setup")]
    public int currentProgress = 0; 
    public Vector3[] viewPositions;
    public float[] phase1XRotations; 

    // 안전하게 값을 가져오기 위한 함수
    public float GetPhase1XRotation()
    {
        if (phase1XRotations != null && phase1XRotations.Length > 0)
        {
            int index = Mathf.Clamp(currentProgress, 0, phase1XRotations.Length - 1);
            return phase1XRotations[index];
        }
        return -90f; // 기본값
    }
    [Header("Video Setup")]
    public VideoPlayer videoPlayer; // 인스펙터에서 비디오 플레이어를 연결할 변수

    void Start()
    {
        // 1. 기존 카메라 위치 설정 로직
        if (viewPositions.Length > 0)
        {
            if (currentProgress >= viewPositions.Length)
            {
                currentProgress = viewPositions.Length - 1;
            }
            transform.position = viewPositions[currentProgress];
        }

        // 2. 비디오 플레이어 초기화 로직
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        
        // 영상 재생이 끝났을 때 이벤트를 연결합니다.
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void Update()
    {
        // 비디오 플레이어가 켜져 있을 때만 스킵 키(엔터, ESC)를 감지합니다.
        if (videoPlayer != null && videoPlayer.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                StopVideo();
            }
        }
    }

    // 영상이 자연스럽게 다 끝났을 때 호출됩니다.
    void OnVideoEnd(VideoPlayer vp)
    {
        StopVideo();
    }

    // 영상을 강제로 끄고 게임 화면을 보여주는 핵심 로직입니다.
    void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.enabled)
        {
            // 메모리 누수 방지를 위해 이벤트 연결을 해제하고 영상을 정지합니다.
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.Stop();

            // 비디오 플레이어 컴포넌트를 꺼서 카메라 앞을 가리던 영상을 치웁니다.
            videoPlayer.enabled = false;
        }
    }
}