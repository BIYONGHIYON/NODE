using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    [Header("References")]
    [Tooltip("비워두셔도 시작 시 자동으로 메인 카메라를 찾습니다.")]
    public TitleCameraSetup cameraSetup; 

    [Header("Position Settings")]
    public Vector3[] shipPositions; 

    private Vector3 startPosition; 
    private Quaternion startRotation;
    private int lastProgress = -1; // 이전 진행도를 저장해서 값이 바뀔 때만 위치 갱신

    void Start()
    {
        // ==========================================
        // [핵심 추가] 씬이 바뀌어 인스펙터 참조가 끊겼거나 비어있을 경우 자동으로 찾아옵니다.
        if (cameraSetup == null && Camera.main != null)
        {
            cameraSetup = Camera.main.GetComponent<TitleCameraSetup>();
        }
        // ==========================================

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