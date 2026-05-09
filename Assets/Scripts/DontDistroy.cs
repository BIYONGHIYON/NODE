using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class DontDistroy : MonoBehaviour
{
    [Header("배경을 숨길 씬 이름들")]
    public string[] hiddenScenes = { "Saturn", "Jupiter", "Mars", "EarthScene" }; 

    private AudioSource audioSource;
    private AudioListener audioListener;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioListener = GetComponent<AudioListener>();
    }

    void OnEnable()
    {
        // 씬이 로드될 때마다 OnSceneLoaded 함수를 실행
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 스크립트가 꺼지거나 파괴될 때 연결 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isHiddenScene = false;

        // 현재 들어온 씬이 숨겨야 하는 씬 목록에 있는지 확인
        for (int i = 0; i < hiddenScenes.Length; i++)
        {
            if (scene.name == hiddenScenes[i])
            {
                isHiddenScene = true;
                break;
            }
        }

        // 1. 자식 오브젝트(실제 모델링이나 이미지)들만 on/off
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(!isHiddenScene);
        }

        // 2. 부모 자신에게 있는 오디오 관련 컴포넌트 제어
        
        if (audioListener != null)
        {
            audioListener.enabled = !isHiddenScene; 
        }

        if (audioSource != null)
        {
            if (isHiddenScene)
            {
                audioSource.Pause(); // 음악 일시정지 (다시 돌아오면 이어서 재생)
            }
            else
            {
                if (!audioSource.isPlaying) audioSource.Play(); // 음악 다시 재생
            }
        }
    }
}