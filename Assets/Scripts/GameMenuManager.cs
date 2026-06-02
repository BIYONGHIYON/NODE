using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Video; 

public class GameMenuManager : MonoBehaviour
{
    [Header("Menu Objects")]
    public GameObject pauseMenuRoot;   
    public GameObject menuType1;       
    public GameObject menuType2;       

    [Header("Volume Sliders")]
    public Slider bgmSlider1; 
    public Slider sfxSlider1;
    public Slider bgmSlider2;
    public Slider sfxSlider2;

    [Header("Audio")]
    public AudioMixer mainMixer;

    private bool isPaused = false;
    private bool isTransitioning = false; 

    void Start()
    {
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);

        // 슬라이더 값이 변할 때마다 자동으로 SetVolume 함수들이 실행되도록 연결!
        if (bgmSlider1) bgmSlider1.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider1) sfxSlider1.onValueChanged.AddListener(SetSFXVolume);
        if (bgmSlider2) bgmSlider2.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider2) sfxSlider2.onValueChanged.AddListener(SetSFXVolume);

        SetupMenuForCurrentScene();
        
        LoadVolumeSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isTransitioning) return; 

            VideoPlayer[] videoPlayers = FindObjectsOfType<VideoPlayer>();
            foreach (VideoPlayer vp in videoPlayers)
            {
                if (vp != null && vp.isPlaying) return; 
            }

            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    void SetupMenuForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "StartScene" || sceneName == "TitleScene" || sceneName == "CharacterScene" || sceneName == "TutorialScene")
        {
            if (menuType1) menuType1.SetActive(true);
            if (menuType2) menuType2.SetActive(false);
        }
        else 
        {
            if (menuType1) menuType1.SetActive(false);
            if (menuType2) menuType2.SetActive(true);
        }
    }

    public void PauseGame()
    {
        if (pauseMenuRoot) pauseMenuRoot.SetActive(true);
        Time.timeScale = 0f; 
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;
    }

    public void GoToTitle()
    {
        if (isTransitioning) return; 
        isTransitioning = true;

        transform.SetParent(null); 
        DontDestroyOnLoad(gameObject);

        StartCoroutine(FadeOutAndLoad("TitleScene"));
    }

    public void ReturnToShip()
    {
        if (isTransitioning) return; 
        isTransitioning = true;
        GameData.justClearedPlanet = false;

        transform.SetParent(null); 
        DontDestroyOnLoad(gameObject);

        StartCoroutine(FadeOutAndLoad("TutorialScene"));
    }

    public void QuitGame() 
    { 
        Debug.Log("게임 종료!");
        Application.Quit(); 
    }

    public void SetBGMVolume(float volume) 
    {
        if (mainMixer != null) mainMixer.SetFloat("BGM", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SavedBGM", volume); 
        
        if (bgmSlider1 != null) bgmSlider1.SetValueWithoutNotify(volume);
        if (bgmSlider2 != null) bgmSlider2.SetValueWithoutNotify(volume);
    }

    public void SetSFXVolume(float volume) 
    {
        if (mainMixer != null) mainMixer.SetFloat("SFX", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SavedSFX", volume); 
        
        if (sfxSlider1 != null) sfxSlider1.SetValueWithoutNotify(volume);
        if (sfxSlider2 != null) sfxSlider2.SetValueWithoutNotify(volume);
    }

    private void LoadVolumeSettings()
    {
        float savedBGM = PlayerPrefs.GetFloat("SavedBGM", 0.4f);
        float savedSFX = PlayerPrefs.GetFloat("SavedSFX", 0.4f);

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);
    }

    private IEnumerator FadeOutAndLoad(string targetSceneName)
    {
        Time.timeScale = 1f; 
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);

        GameObject fadeObj = new GameObject("PersistentFadeCanvas");
        DontDestroyOnLoad(fadeObj);

        Canvas fadeCanvas = fadeObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; 

        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); 

        float fadeDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, elapsed / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black; 

        Camera persistentCam = null;
        Camera[] allCameras = FindObjectsOfType<Camera>(true); 
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.scene.name == "DontDestroyOnLoad")
            {
                persistentCam = cam;
                persistentCam.gameObject.SetActive(true); 
                break;
            }
        }

        SceneManager.LoadScene(targetSceneName);

        yield return null;
        yield return null;

        if (targetSceneName == "TitleScene" && persistentCam != null)
        {
            Camera[] currentCameras = FindObjectsOfType<Camera>();
            foreach(Camera cam in currentCameras)
            {
                if (cam != persistentCam && cam.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    Destroy(cam.gameObject);
                }
            }
        }

        if (targetSceneName == "TutorialScene")
        {
            MovingAst[] players = FindObjectsOfType<MovingAst>();
            foreach(var p in players) if (p != null) p.enabled = false;
            
            yield return new WaitForSeconds(3.6f); 
            
            players = FindObjectsOfType<MovingAst>();
            foreach(var p in players) if (p != null) p.enabled = true;
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
            yield return null;
        }

        Destroy(fadeObj);
        Destroy(gameObject);
    }
}