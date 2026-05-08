using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class DontDistroy : MonoBehaviour
{
    [Header("배경을 숨길 씬 이름들")]
    [Tooltip("여기에 적힌 씬에 들어가면 배경이 숨겨집니다.")]
    public string[] hiddenScenes = { "Saturn", "Jupiter", "Mars", "EarthScene" }; 

    // [추가됨] 부모 자신에게 있는 오디오 컴포넌트를 담을 변수
    private AudioSource audioSource;
    private AudioListener audioListener;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 내 오브젝트에 있는 오디오 컴포넌트들을 찾아둡니다.
        audioSource = GetComponent<AudioSource>();
        audioListener = GetComponent<AudioListener>();
    }

    void OnEnable()
    {
        // 씬이 로드될 때마다 OnSceneLoaded 함수를 실행하도록 연결합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 스크립트가 꺼지거나 파괴될 때 연결을 해제합니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isHiddenScene = false;

        // 현재 들어온 씬이 숨겨야 하는 씬 목록에 있는지 확인합니다.
        for (int i = 0; i < hiddenScenes.Length; i++)
        {
            if (scene.name == hiddenScenes[i])
            {
                isHiddenScene = true;
                break;
            }
        }

        // 1. 자식 오브젝트(실제 모델링이나 이미지)들만 켜거나 끕니다.
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(!isHiddenScene);
        }

        // ==========================================
        // 2. [추가됨] 부모 자신에게 있는 오디오 관련 컴포넌트 제어
        
        // 해당 씬에서 배경을 숨겨야 한다면 귀(Listener)도 꺼서 충돌을 막습니다.
        if (audioListener != null)
        {
            audioListener.enabled = !isHiddenScene; 
        }

        // 해당 씬에서 배경을 숨겨야 한다면 배경음악(Source)도 끕니다.
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
        // ==========================================
    }
}