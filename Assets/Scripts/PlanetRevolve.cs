using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRevolve : MonoBehaviour
{
    [SerializeField]
    GameObject fixedStar;
    [SerializeField]
    float revolutionSpeed = 0.5f;               // 공전속도
    [SerializeField]
    public float eccentricity = 0.0167f;        // 편심

    float majorAxisRound;                       // 장축 반지름 (x축)
    float minorAxisRound;                       // 단축 반지름 (z축)
    float angle = 0;                            // 현재 각도
    
    void Start()
    {
        // 중심 오브젝트 (항성) 지정 -> 태양계 : 태양
        fixedStar = GameObject.Find("Sun");

        // 장축 반지름은 Object transform.x좌표로 설정
        majorAxisRound = this.transform.position.x;

        // 유니티 환경과 실제 환경이 다르게 설정되어 있으므로 편심을 기준으로 장축 반지름, 단축반지름을 설정.
        // 편심 값을 고정으로, 비율에 따라 Inspector에서 장축 반지름을 세팅
        // 단축 반지름은 편심을 기준으로 자동 계산하여 세팅
        minorAxisRound = majorAxisRound * Mathf.Sqrt(1 - eccentricity * eccentricity);
    }

    void Update()
    {
        //// 중심 오브젝트 기준 회전 (원 궤도)
        //transform.RotateAround(fixedStar.transform.position, Vector3.up, revolutionSpeed * Time.deltaTime);

        // 각도 변환
        // 매 프레임마다 일정하게 증가 -> 행성이 궤도를 따라 일정하게 이동
        angle += revolutionSpeed * Time.deltaTime;

        // angle 범위 바운더리 설정 (2π 이내로)
        if (angle > Mathf.PI * 2) angle -= Mathf.PI * 2;

        // 중력에 의해 타원 궤도의 중심이 항상 한 초점을 향하도록 계산
        // 타원의 중심 ~ 초점까지의 거리 = 편심 * 장축 반지름
        float offsetX = eccentricity * majorAxisRound;
        
        // 각도를 기준으로 타원 상 위치 계산 (타원 방정식)
        float x = Mathf.Cos(angle) * majorAxisRound;
        float z = Mathf.Sin(angle) * minorAxisRound;

        // 타원 궤도 공전 (한 초점을 기준으로 설정)
        Vector3 orbitPosition = new Vector3(x - offsetX, 0, z);
        transform.position = fixedStar.transform.position + orbitPosition;
    }
}