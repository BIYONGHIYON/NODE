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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isHiddenScene = false;

        for (int i = 0; i < hiddenScenes.Length; i++)
        {
            if (scene.name == hiddenScenes[i])
            {
                isHiddenScene = true;
                break;
            }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(!isHiddenScene);
        }

        
        if (audioListener != null)
        {
            audioListener.enabled = !isHiddenScene; 
        }

        if (audioSource != null)
        {
            if (isHiddenScene)
            {
                audioSource.Pause();
            }
            else
            {
                if (!audioSource.isPlaying) audioSource.Play();
            }
        }
    }
}