using System.Collections;
using UnityEngine;

public class InteractionBox : MonoBehaviour
{
    [Header("References")]
    public press1 promptText; 
    public GameObject uiCanvas; 
    public Transform cameraUiView; 
    
    // [추가됨] 인스펙터에서 직접 카메라 셋업 스크립트를 끌어다 넣을 수 있게 public으로 변경합니다.
    public TutorialCameraSetup cameraSetup; 

    [Header("Settings")]
    public float cameraMoveDuration = 1f; 
    public KeyCode interactKeyP1 = KeyCode.F;
    public KeyCode interactKeyP2_1 = KeyCode.RightControl;
    public KeyCode interactKeyP2_2 = KeyCode.RightAlt;

    private bool isPlayerInZone = false;
    private bool isInteracting = false;
    
    private Camera mainCam;
    
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    private MovingAst[] players;

    void Start()
    {
        mainCam = Camera.main;
        
        // 인스펙터에서 수동으로 넣지 않았다면 메인 카메라에서 찾아봅니다.
        if (cameraSetup == null && mainCam != null)
        {
            cameraSetup = mainCam.GetComponent<TutorialCameraSetup>();
        }

        // 못 찾았을 경우 콘솔창에 경고를 띄워 알려줍니다.
        if (cameraSetup == null)
        {
            Debug.LogWarning("[InteractionBox] TutorialCameraSetup을 찾을 수 없습니다! 인스펙터 창에서 직접 연결해 주세요.");
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (uiCanvas != null) uiCanvas.SetActive(false);

        players = FindObjectsOfType<MovingAst>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInteracting)
        {
            isPlayerInZone = true;
            if (promptText != null) promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (promptText != null && !isInteracting) 
                promptText.gameObject.SetActive(false); 
        }
    }

    void Update()
    {
        if (isPlayerInZone && !isInteracting)
        {
            if (Input.GetKeyDown(interactKeyP1) || Input.GetKeyDown(interactKeyP2_1) || Input.GetKeyDown(interactKeyP2_2))
            {
                StartInteraction();
            }
        }
    }

    public void StartInteraction()
    {
        isInteracting = true;
        
        if (promptText != null) promptText.gameObject.SetActive(false);

        SetPlayersMovement(false);

        // 3. 기존 카메라 트래킹 중지 (이제 확실하게 꺼집니다!)
        if (cameraSetup != null) 
        {
            cameraSetup.enabled = false;
        }

        StartCoroutine(MoveCameraAndShowUI());
    }

    public void EndInteraction()
    {
        if (uiCanvas != null) uiCanvas.SetActive(false);
        StartCoroutine(ReturnCameraAndResume());
    }

    private IEnumerator MoveCameraAndShowUI()
    {
        originalCamPos = mainCam.transform.position;
        originalCamRot = mainCam.transform.rotation;

        Vector3 targetPos = cameraUiView.position;
        Quaternion targetRot = cameraUiView.rotation;

        float elapsed = 0f;
        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / cameraMoveDuration);
            mainCam.transform.position = Vector3.Lerp(originalCamPos, targetPos, t);
            mainCam.transform.rotation = Quaternion.Slerp(originalCamRot, targetRot, t);
            yield return null;
        }

        mainCam.transform.position = targetPos;
        mainCam.transform.rotation = targetRot;

        if (uiCanvas != null) uiCanvas.SetActive(true);
    }

    private IEnumerator ReturnCameraAndResume()
    {
        Vector3 currentPos = mainCam.transform.position;
        Quaternion currentRot = mainCam.transform.rotation;

        float elapsed = 0f;
        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / cameraMoveDuration);
            mainCam.transform.position = Vector3.Lerp(currentPos, originalCamPos, t);
            mainCam.transform.rotation = Quaternion.Slerp(currentRot, originalCamRot, t);
            yield return null;
        }

        mainCam.transform.position = originalCamPos;
        mainCam.transform.rotation = originalCamRot;

        // 1. 카메라 트래킹 스크립트 다시 켜기
        if (cameraSetup != null) 
        {
            cameraSetup.enabled = true;
        }

        SetPlayersMovement(true);
        isInteracting = false;

        if (isPlayerInZone && promptText != null)
        {
            promptText.gameObject.SetActive(true);
        }
    }

    private void SetPlayersMovement(bool canMove)
    {
        foreach (MovingAst player in players)
        {
            if (player != null)
            {
                player.enabled = canMove;
                
                if (!canMove)
                {
                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    
                    Animator anim = player.GetComponentInChildren<Animator>();
                    if (anim != null) anim.SetBool("isMoving", false);
                }
            }
        }
    }
}