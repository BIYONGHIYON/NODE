using System.Collections;
using UnityEngine;

public class InteractionBox : MonoBehaviour
{
    [Header("References")]
    public press1 promptText; 
    public GameObject uiCanvas; 
    public Transform cameraUiView; 
    
    public TutorialCameraSetup cameraSetup; 

    [Header("Settings")]
    public float cameraMoveDuration = 1f; 
    public KeyCode interactKeyP1 = KeyCode.F;
    public KeyCode interactKeyP2_1 = KeyCode.RightControl;
    public KeyCode interactKeyP2_2 = KeyCode.RightAlt;

    // ==========================================
    // [추가됨] 카메라 이동 효과음 설정
    [Header("SFX Settings")]
    public AudioSource sfxSource;
    public AudioClip cameraMoveSound;
    // ==========================================

    private bool isPlayerInZone = false;
    private bool isInteracting = false;
    
    private Camera mainCam;
    
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    private MovingAst[] players;

    void Start()
    {
        mainCam = Camera.main;
        
        if (cameraSetup == null && mainCam != null)
        {
            cameraSetup = mainCam.GetComponent<TutorialCameraSetup>();
        }

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
        // ==========================================
        // [추가됨] UI로 카메라가 이동하기 시작할 때 효과음 재생
        if (sfxSource != null && cameraMoveSound != null)
        {
            sfxSource.PlayOneShot(cameraMoveSound);
        }
        // ==========================================

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
        // ==========================================
        // [추가됨] 원래 위치로 카메라가 돌아가기 시작할 때 효과음 재생
        if (sfxSource != null && cameraMoveSound != null)
        {
            sfxSource.PlayOneShot(cameraMoveSound);
        }
        // ==========================================

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