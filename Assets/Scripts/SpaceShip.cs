using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    [Header("References")]
    // 인스펙터에서 메인 카메라를 여기에 드래그해서 넣으세요.
    public TitleCameraSetup cameraSetup; 

    [Header("Position Settings")]
    public Vector3[] shipPositions; 


    private Vector3 startPosition; 
    private Quaternion startRotation;
    private int lastProgress = -1; // 이전 진행도를 저장해서 값이 바뀔 때만 위치 갱신

    void Start()
    {
        startRotation = transform.rotation;
        UpdateShipPosition(); // 시작할 때 한 번 위치 설정
    }

    void Update()
    {
        // 1. 카메라의 currentProgress가 바뀌었는지 체크
        if (cameraSetup != null && cameraSetup.currentProgress != lastProgress)
        {
            UpdateShipPosition();
        }
    }

    void UpdateShipPosition()
    {
        if (cameraSetup == null || shipPositions.Length == 0) return;

        // 카메라의 변수를 가져옴
        int index = Mathf.Clamp(cameraSetup.currentProgress, 0, shipPositions.Length - 1);
        
        // 우주선의 '기준 위치'를 갱신
        startPosition = shipPositions[index];
        lastProgress = cameraSetup.currentProgress;
        transform.position = startPosition;
    }
}