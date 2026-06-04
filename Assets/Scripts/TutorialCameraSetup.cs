using System.Collections;
using UnityEngine;

public class TutorialCameraSetup : MonoBehaviour
{
    [Header("References")]
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

    IEnumerator Start()
    {
        while (playerManager == null || playerManager.character1 == null || playerManager.character2 == null)
        {
            yield return null;
        }

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            TitleCameraSetup titleSetup = mainCam.GetComponent<TitleCameraSetup>();
            if (titleSetup != null && titleSetup.viewPositions.Length > 0)
            {
                // GameData에서 정확한 진행도를 가져옵니다.
                int index = Mathf.Clamp(GameData.currentProgress, 0, titleSetup.viewPositions.Length - 1);
                Vector3 finalCamPos = titleSetup.viewPositions[index];
                Quaternion finalCamRot = Quaternion.Euler(targetRotation);

                Matrix4x4 camMatrix = Matrix4x4.TRS(finalCamPos, finalCamRot, Vector3.one);

                if (spaceshipObject != null)
                {
                    spaceshipObject.position = camMatrix.MultiplyPoint3x4(localPositionOffset);
                    spaceshipObject.rotation = finalCamRot * Quaternion.Euler(localRotationOffset);
                }

                StartCoroutine(SmoothTransition(mainCam.transform, finalCamPos, targetRotation, camMatrix, finalCamRot));
            }
        }
    }

    IEnumerator SmoothTransition(Transform camTransform, Vector3 endPos, Vector3 endRotEuler, Matrix4x4 camMatrix, Quaternion camRot)
    {
        if (sfxSource != null && cameraMoveSound != null)
        {
            sfxSource.PlayOneShot(cameraMoveSound);
        }

        CharacterController cc1 = playerManager.character1.GetComponent<CharacterController>();
        CharacterController cc2 = playerManager.character2.GetComponent<CharacterController>();
        Rigidbody rb1 = playerManager.character1.GetComponent<Rigidbody>();
        Rigidbody rb2 = playerManager.character2.GetComponent<Rigidbody>();

        if (cc1 != null) cc1.enabled = false;
        if (cc2 != null) cc2.enabled = false;
        if (rb1 != null) rb1.isKinematic = true;
        if (rb2 != null) rb2.isKinematic = true;

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
            LockCharactersPosition(camMatrix, camRot);
            yield return null;
        }
        camTransform.rotation = phase1Rot;

        elapsed = 0f;
        while (elapsed < 2.0f)
        {
            elapsed += Time.deltaTime;
            camTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, elapsed / 2.0f));
            LockCharactersPosition(camMatrix, camRot);
            yield return null;
        }
        camTransform.position = endPos;

        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            camTransform.rotation = Quaternion.Slerp(phase1Rot, finalRot, Mathf.SmoothStep(0f, 1f, elapsed / 0.5f));
            LockCharactersPosition(camMatrix, camRot);
            yield return null;
        }
        camTransform.rotation = finalRot;

        LockCharactersPosition(camMatrix, camRot);

        if (cc1 != null) cc1.enabled = true;
        if (cc2 != null) cc2.enabled = true;
        if (rb1 != null) rb1.isKinematic = false;
        if (rb2 != null) rb2.isKinematic = false;

        if (playerManager != null)
        {
            playerManager.isTracking = true;
        }
    }

    void LockCharactersPosition(Matrix4x4 camMatrix, Quaternion camRot)
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
}