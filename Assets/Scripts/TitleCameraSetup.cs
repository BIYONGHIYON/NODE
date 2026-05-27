using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리 추가
using UnityEngine.Video;

public class TitleCameraSetup : MonoBehaviour
{
    [Header("Camera Setup")]
    public int currentProgress = 0; 
    public Vector3[] viewPositions;
    public float[] phase1XRotations; 
    
    private Quaternion initialRotation; // 카메라의 초기 회전값 저장용
    [Header("Scene Transition")]
    public string nextSceneName = "TitleScene"; 

    public float GetPhase1XRotation()
    {
        if (phase1XRotations != null && phase1XRotations.Length > 0)
        {
            int index = Mathf.Clamp(currentProgress, 0, phase1XRotations.Length - 1);
            return phase1XRotations[index];
        }
        return -90f;
    }

    [Header("Video Setup")]
    public VideoPlayer videoPlayer;

    [Header("Audio Setup")]
    public AudioSource bgmSource;

    void Awake()
    {
        // StartScene일 때 카메라의 기본 회전값
        initialRotation = transform.rotation;
        
        // 씬이 로드될 때마다 실행할 함수
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Awake - OnSceneLoaded가 최초 1회 꼬일 경우를 대비한 안전장치
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            SetupCameraPosition();
            PlayIntroVideo();
            GameData.justClearedPlanet = false; // 타이틀 화면에 진입할 때마다 초기화
        }
    }

    // 씬이 변경될 때마다 자동 호출
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TitleScene")
        {
            // 메뉴를 통해 TitleScene으로 돌아왔을 때의 처리
            GameData.justClearedPlanet = false; // 타이틀 화면에 진입할 때마다 초기화
            SetupCameraPosition();
            SkipIntroVideo();
        }
    }

    void SetupCameraPosition()
    {
        currentProgress = GameData.currentProgress;

        if (viewPositions.Length > 0)
        {
            if (currentProgress >= viewPositions.Length)
            {
                currentProgress = viewPositions.Length - 1;
            }
            
            // 카메라 위치와 회전값을 무조건 타이틀 화면 상태로 원상복구
            transform.position = viewPositions[currentProgress];
            transform.rotation = initialRotation; 
        }
    }

    void PlayIntroVideo()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.enabled = true;
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play(); 
        }
    }

    void SkipIntroVideo()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.enabled = false;
        }
        PlayBGM(); // 영상 없이 바로 BGM 재생
    }

    void Update()
    {
        if (videoPlayer != null && videoPlayer.enabled && videoPlayer.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                StopVideo();
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        StopVideo();
    }

    void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.enabled)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.Stop();
            videoPlayer.enabled = false;
        }

        PlayBGM();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void PlayBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }
}