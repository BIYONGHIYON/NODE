using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 카메라 이동
    [SerializeField]
    float _zoomSpeed = 300f;
    [SerializeField]
    float _zoomMax = 200f;
    [SerializeField]
    float _zoomMin = 1000f;

    [SerializeField]
    float _RotateSpeed = -1f;
    [SerializeField]
    float _dragSpeed = 10f;
    [SerializeField]
    float _inputSpeed = 20f;

    // 카메라 이동 제한
    [SerializeField]
    float radius;
    [SerializeField]
    GameObject _DrawConstellation;

    void Start()
    {
        // 카메라 이동 범위 -> 별자리 구 안쪽으로 세팅
        radius = _DrawConstellation.GetComponent<DrawConstellation>().SpaceSize;
        radius -= 100.0f;

    }
    
    void LateUpdate()
    {
        CameraZoom();
        CameraDrag();
        CameraRotate();
        CameraInput();

        CameraBoundary();
    }

    void CameraRotate()
    {
        if (Input.GetMouseButton(1))
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            Vector3 rotateValue = new Vector3(y, x * -1, 0);
            transform.eulerAngles = transform.eulerAngles - rotateValue;
            transform.eulerAngles += rotateValue * _RotateSpeed;
        }
    }

    void CameraZoom()
    {
        float _zoomDirection = Input.GetAxis("Mouse ScrollWheel");

        if (transform.position.y - _zoomDirection * _zoomSpeed <= _zoomMax && _zoomDirection > 0)
            return;

        if (transform.position.y - _zoomDirection * _zoomSpeed >= _zoomMin && _zoomDirection < 0)
            return;

        transform.position += transform.forward * _zoomDirection * _zoomSpeed * Time.deltaTime;
    }

    void CameraDrag()
    {
        if (Input.GetMouseButton(0))
        {
            float posX = Input.GetAxis("Mouse X");
            float posZ = Input.GetAxis("Mouse Y");

            Quaternion v3Rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            transform.position += v3Rotation * new Vector3(posX * -_dragSpeed, 0, posZ * -_dragSpeed);
        }
    }

    float totalRun = 1.0f;
    void CameraInput()
    {
        Vector3 p_Velocity = new Vector3();
        // 유니티 씬뷰와 동일하게 입력키 이동 변경
        // Q/E : 현 시점 아래/위     W/S : 현 시점 앞/뒤    A/D : 현 시점 좌/우
        if (Input.GetKey(KeyCode.E))
            p_Velocity += new Vector3(0, 1f, 0);
        if (Input.GetKey(KeyCode.Q))
            p_Velocity += new Vector3(0, -1f, 0);
        if (Input.GetKey(KeyCode.W))
            transform.position += transform.forward * _zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            transform.position -= transform.forward * _zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Alpha1))
            p_Velocity += new Vector3(0, 0, 1f);
        if (Input.GetKey(KeyCode.Alpha2))
            p_Velocity += new Vector3(0, 0, -1f);
        if (Input.GetKey(KeyCode.A))
            p_Velocity += new Vector3(-1f, 0, 0);
        if (Input.GetKey(KeyCode.D))
            p_Velocity += new Vector3(1f, 0, 0);

        Vector3 p = p_Velocity;
        if (p.sqrMagnitude > 0)
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * 1.0f;

            p.x = Mathf.Clamp(p.x, -_inputSpeed, _inputSpeed);
            p.y = Mathf.Clamp(p.y, -_inputSpeed, _inputSpeed);
            p.z = Mathf.Clamp(p.z, -_inputSpeed, _inputSpeed);

            transform.Translate(p);
        }
    }

    void CameraBoundary()
    {
        Vector3 offset = transform.position - Vector3.zero;
        if (offset.magnitude > radius)
        {
            // 반경을 초과할 경우 반경 안쪽으로 위치 고정
            offset = offset.normalized * radius;
            transform.position = Vector3.zero + offset;
        }
    }
}