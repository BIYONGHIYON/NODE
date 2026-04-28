using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    [SerializeField]
    float rotationSpeed = 15f;      // 자전속도
    [SerializeField]
    float rotationAxis = -23.0f;    // 자전축

    void Awake()
    {
        transform.eulerAngles = new Vector3(0, 0, rotationAxis);
    }

    void Update()
    {
        // 자전 구현 (반시계 방향 회전 기준) 
        transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);
    }
}

