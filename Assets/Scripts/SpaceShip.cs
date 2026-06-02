using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    [Header("References")]
    public TitleCameraSetup cameraSetup; 

    [Header("Position Settings")]
    public Vector3[] shipPositions; 

    private Vector3 startPosition; 
    private Quaternion startRotation;
    private int lastProgress = -1;

    void Start()
    {
        if (cameraSetup == null && Camera.main != null)
        {
            cameraSetup = Camera.main.GetComponent<TitleCameraSetup>();
        }

        startRotation = transform.rotation;
        UpdateShipPosition(); 
    }

    void Update()
    {
        if (cameraSetup != null && cameraSetup.currentProgress != lastProgress)
        {
            UpdateShipPosition();
        }
    }

    void UpdateShipPosition()
    {
        if (cameraSetup == null || shipPositions.Length == 0) return;

        int index = Mathf.Clamp(cameraSetup.currentProgress, 0, shipPositions.Length - 1);
        
        startPosition = shipPositions[index];
        lastProgress = cameraSetup.currentProgress;
        transform.position = startPosition;
    }
}