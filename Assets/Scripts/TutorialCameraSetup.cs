using System.Collections;
using UnityEngine;

public class TutorialCameraSetup : MonoBehaviour
{
    [Header("References")]
    // 분리된 범용 매니저 스크립트를 연결할 변수
    public CoopPlayerManager playerManager; 

    [Header("SFX Settings")]
    public AudioSource sfxSource;
    public AudioClip cameraMoveSound;

    [Header("Camera Settings")]
    public Vector3 targetRotation = new Vector3(15f, 70f, 0f);

    [Header("Spaceship Settings")]
    public Transform spaceshipObject;
    public Vector3 localPositionOffset = new Vector3(0f, 0f, 10f); 
    public Vector3 localRotationOffset = Vector3.zero;

    [Space(10)]
    [Header("Character 1 Settings (왼쪽 자리)")]
    public Vector3 char1LocalOffset = new Vector3(-1.5f, 0f, 15f); 
    public Vector3 char1LocalRotation = Vector3.zero;
    public Vector3 char1LocalScale = Vector3.one; 
    
    [Space(5)]
    [Header("Character 2 Settings (오른쪽 자리)")]
    public Vector3 char2LocalOffset = new Vector3(1.5f, 0f, 15f); 
    public Vector3 char2LocalRotation = Vector3.zero;
    public Vector3 char2LocalScale = Vector3.one; 

    void Start()
    {
        Camera mainCam = Camera.main;

        if (mainCam != null && playerManager != null)
        {
            TitleCameraSetup titleSetup = mainCam.GetComponent<TitleCameraSetup>();
            if (titleSetup != null && titleSetup.viewPositions.Length > 0)
            {
                int index = Mathf.Clamp(titleSetup.currentProgress, 0, titleSetup.viewPositions.Length - 1);
                Vector3 finalCamPos = titleSetup.viewPositions[index];
                Quaternion finalCamRot = Quaternion.Euler(targetRotation);

                Matrix4x4 camMatrix = Matrix4x4.TRS(finalCamPos, finalCamRot, Vector3.one);

                if (spaceshipObject != null)
                {
                    spaceshipObject.position = camMatrix.MultiplyPoint3x4(localPositionOffset);
                    spaceshipObject.rotation = finalCamRot * Quaternion.Euler(localRotationOffset);
                }

                PlaceCharactersRelativeToCamera(camMatrix, finalCamRot);

                StartCoroutine(SmoothTransition(mainCam.transform, finalCamPos, targetRotation));
            }
        }
    }

    void PlaceCharactersRelativeToCamera(Matrix4x4 camMatrix, Quaternion camRot)
    {
        if (playerManager.character1 != null)
        {
            playerManager.character1.position = camMatrix.MultiplyPoint3x4(char1LocalOffset);
            playerManager.character1.rotation = camRot * Quaternion.Euler(char1LocalRotation);
            playerManager.character1.localScale = char1LocalScale;
        }

        if (playerManager.character2 != null)
        {
            playerManager.character2.position = camMatrix.MultiplyPoint3x4(char2LocalOffset);
            playerManager.character2.rotation = camRot * Quaternion.Euler(char2LocalRotation);
            playerManager.character2.localScale = char2LocalScale;
        }
    }

    IEnumerator SmoothTransition(Transform camTransform, Vector3 endPos, Vector3 endRotEuler)
    {
        if (sfxSource != null && cameraMoveSound != null)
        {
            sfxSource.PlayOneShot(cameraMoveSound);
        }

        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;
        
        float targetX = -90f;
        TitleCameraSetup titleSetup = camTransform.GetComponent<TitleCameraSetup>();
        if (titleSetup != null) targetX = titleSetup.GetPhase1XRotation();

        Vector3 currentEuler = startRot.eulerAngles;
        Quaternion phase1Rot = Quaternion.Euler(targetX, currentEuler.y, currentEuler.z);
        
        Quaternion finalRot = Quaternion.Euler(endRotEuler);

        float elapsed = 0f;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            camTransform.rotation = Quaternion.Slerp(startRot, phase1Rot, Mathf.SmoothStep(0f, 1f, elapsed / 1.0f));
            yield return null;
        }
        camTransform.rotation = phase1Rot;

        elapsed = 0f;
        while (elapsed < 2.0f)
        {
            elapsed += Time.deltaTime;
            camTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, elapsed / 2.0f));
            yield return null;
        }
        camTransform.position = endPos;

        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            camTransform.rotation = Quaternion.Slerp(phase1Rot, finalRot, Mathf.SmoothStep(0f, 1f, elapsed / 0.5f));
            yield return null;
        }
        camTransform.rotation = finalRot;

        if (playerManager != null)
        {
            playerManager.isTracking = true;
        }
    }
}