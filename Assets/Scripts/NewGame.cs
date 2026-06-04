using UnityEngine;
using UnityEngine.SceneManagement; 

public class NewGame : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("숨기거나 보여줄 'New Game' 버튼 오브젝트를 드래그해서 넣으세요.")]
    public GameObject newGameButton;

    void Start()
    {
        if (newGameButton != null)
        {
            if (GameData.currentProgress == 0)
            {
                newGameButton.SetActive(false);
            }
            else
            {
                newGameButton.SetActive(true);
            }
        }
    }

    public void StartNewGame()
    {
        PlayerPrefs.SetInt("SaveProgress", 0);
        PlayerPrefs.Save();

        GameData.currentProgress = 0;
        GameData.justClearedPlanet = false;
        GameData.isFuelAcquired = false;

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}