using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TitleCameraSetup : MonoBehaviour
{
    [Header("Camera Setup")]
    public int currentProgress = 0;
    public Vector3[] viewPositions;
    public float[] phase1XRotations;
    private Quaternion initialRotation;
    
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
        initialRotation = transform.rotation;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            SetupCameraPosition();
            PlayIntroVideo();
            GameData.justClearedPlanet = false;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ========================================================
        // [버그 수정] GameMenuManager 때문에 시간이 멈춰있을 수 있으므로 강제로 다시 켭니다!
        Time.timeScale = 1f; 
        // ========================================================

        if (scene.name == "TitleScene")
        {
            GameData.justClearedPlanet = false;
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
        PlayBGM();
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