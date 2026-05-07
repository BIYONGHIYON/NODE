using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가

public class DontDistroy : MonoBehaviour
{
    [Header("배경을 숨길 씬 이름들")]
    [Tooltip("여기에 적힌 씬에 들어가면 배경이 숨겨집니다.")]
    public string[] hiddenScenes = { "Saturn", "Jupiter", "Mars", "EarthScene" }; 

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
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

        // 자기 자신(스크립트가 붙은 부모)을 끄면 스크립트가 정지되므로,
        // 내 아래에 있는 자식 오브젝트(실제 모델링이나 이미지)들만 켜거나 끕니다.
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(!isHiddenScene);
        }
    }
}