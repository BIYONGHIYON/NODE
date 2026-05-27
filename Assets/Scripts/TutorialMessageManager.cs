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

        // GameData에 '정상 클리어' 도장이 찍혀 있다면?
        if (GameData.justClearedPlanet)
        {
            // 도장을 지워주고 코루틴 시작!
            GameData.justClearedPlanet = false; 
            StartCoroutine(ShowMessageRoutine());
        }
    }

    IEnumerator ShowMessageRoutine()
    {
        // 1. 카메라 연출(3.6초) + 페이드인(약 1초)이 끝날 때까지 어둠 속에서 조용히 기다립니다.
        yield return new WaitForSeconds(initialDelay);

        // 2. 화면이 완전히 밝아지면 그제야 짠! 하고 등장합니다.
        if (clearMessageUI != null)
        {
            clearMessageUI.SetActive(true);
            yield return new WaitForSeconds(displayDuration);
            clearMessageUI.SetActive(false);
        }
    }
}