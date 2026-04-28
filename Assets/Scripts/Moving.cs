using UnityEngine;

public class Moving : MonoBehaviour
{
    [Header("Random Movement Settings")]
    public float moveSpeed = 0.5f;
    public float moveRange = 1.0f; // 움직일 반경 설정
    public float rotationSpeed = 0.3f;
    public float rotationAmount = 15.0f;

    private Vector3 startPosition; 
    private Quaternion startRotation;

    // Perlin Noise가 항상 같은 패턴을 내지 않도록 축마다 시작점을 다르게 줍니다.
    private float randomOffsetX;
    private float randomOffsetY;
    private float randomOffsetZ;

    void Start()
    {
        startPosition = transform.position; 
        startRotation = transform.rotation;

        // 게임을 실행할 때마다 고유한 움직임 패턴을 가지도록 임의의 오프셋 부여
        randomOffsetX = Random.Range(0f, 100f);
        randomOffsetY = Random.Range(0f, 100f);
        randomOffsetZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        // 1. 위치 랜덤 이동 (Mathf.PerlinNoise는 0~1 값을 반환하므로 *2 -1을 하여 -1~1 값으로 변환)
        float noiseX = (Mathf.PerlinNoise(Time.time * moveSpeed + randomOffsetX, 0f) * 2f - 1f) * moveRange;
        float noiseY = (Mathf.PerlinNoise(0f, Time.time * moveSpeed + randomOffsetY) * 2f - 1f) * moveRange;
        float noiseZ = (Mathf.PerlinNoise(Time.time * moveSpeed + randomOffsetZ, Time.time * moveSpeed + randomOffsetZ) * 2f - 1f) * moveRange;

        transform.position = startPosition + new Vector3(noiseX, noiseY, noiseZ);

        // 2. 회전 랜덤 기울기 (위치 이동과는 다른 패턴을 위해 오프셋에 임의의 숫자를 더함)
        float rotNoiseX = (Mathf.PerlinNoise(Time.time * rotationSpeed + randomOffsetX + 50f, 0f) * 2f - 1f) * rotationAmount;
        float rotNoiseY = (Mathf.PerlinNoise(0f, Time.time * rotationSpeed + randomOffsetY + 50f) * 2f - 1f) * rotationAmount;
        float rotNoiseZ = (Mathf.PerlinNoise(Time.time * rotationSpeed + randomOffsetZ + 50f, Time.time * rotationSpeed + randomOffsetZ + 50f) * 2f - 1f) * rotationAmount;

        transform.rotation = startRotation * Quaternion.Euler(rotNoiseX, rotNoiseY, rotNoiseZ);
    }
}