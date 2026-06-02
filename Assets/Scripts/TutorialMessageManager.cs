using System.Collections;
using UnityEngine;

public class TutorialMessageManager : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("'이제 다음 행성을 갈 수 있습니다' 문구가 적힌 UI 오브젝트를 넣으세요.")]
    public GameObject clearMessageUI;
    
    [Tooltip("문구가 화면에 머무는 시간입니다.")]
    public float displayDuration = 3f;

    [Tooltip("씬이 시작되고 문구가 뜨기 전까지 기다릴 시간입니다. (연출 시간 고려)")]
    public float initialDelay = 4.5f; 

    void Start()
    {
        if (clearMessageUI != null) clearMessageUI.SetActive(false);

        if (GameData.justClearedPlanet)
        {
            GameData.justClearedPlanet = false; 
            StartCoroutine(ShowMessageRoutine());
        }
    }

    IEnumerator ShowMessageRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        if (clearMessageUI != null)
        {
            clearMessageUI.SetActive(true);
            yield return new WaitForSeconds(displayDuration);
            clearMessageUI.SetActive(false);
        }
    }
}