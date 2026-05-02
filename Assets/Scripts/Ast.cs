using UnityEngine;

public class Ast : MonoBehaviour
{
    [Header("Random Movement Settings")]
    public float moveSpeed = 0.5f;
    public float moveRange = 1.0f; 
    public float rotationSpeed = 0.3f;
    public float rotationAmount = 15.0f;

    private Vector3 startLocalPosition; 
    private Quaternion startLocalRotation;

    private float randomOffsetX;
    private float randomOffsetY;
    private float randomOffsetZ;

    // 시작 시점의 노이즈 값을 저장할 변수들
    private float initialNoiseX;
    private float initialNoiseY;
    private float initialNoiseZ;

    private float initialRotX;
    private float initialRotY;
    private float initialRotZ;

    void Start()
    {
        startLocalPosition = transform.localPosition; 
        startLocalRotation = transform.localRotation;

        randomOffsetX = Random.Range(0f, 100f);
        randomOffsetY = Random.Range(0f, 100f);
        randomOffsetZ = Random.Range(0f, 100f);

        // 첫 프레임에 튈 노이즈 값을 미리 계산해 둡니다.
        initialNoiseX = (Mathf.PerlinNoise(randomOffsetX, 0f) * 2f - 1f) * moveRange;
        initialNoiseY = (Mathf.PerlinNoise(0f, randomOffsetY) * 2f - 1f) * moveRange;
        initialNoiseZ = (Mathf.PerlinNoise(randomOffsetZ, randomOffsetZ) * 2f - 1f) * moveRange;

        initialRotX = (Mathf.PerlinNoise(randomOffsetX + 50f, 0f) * 2f - 1f) * rotationAmount;
        initialRotY = (Mathf.PerlinNoise(0f, randomOffsetY + 50f) * 2f - 1f) * rotationAmount;
        initialRotZ = (Mathf.PerlinNoise(randomOffsetZ + 50f, randomOffsetZ + 50f) * 2f - 1f) * rotationAmount;
    }

    void Update()
    {
        float noiseX = (Mathf.PerlinNoise(Time.time * moveSpeed + randomOffsetX, 0f) * 2f - 1f) * moveRange;
        float noiseY = (Mathf.PerlinNoise(0f, Time.time * moveSpeed + randomOffsetY) * 2f - 1f) * moveRange;
        float noiseZ = (Mathf.PerlinNoise(Time.time * moveSpeed + randomOffsetZ, Time.time * moveSpeed + randomOffsetZ) * 2f - 1f) * moveRange;

        // 현재 노이즈에서 시작 노이즈를 빼주어 첫 프레임 이동량이 0이 되도록 만듭니다.
        float finalX = noiseX - initialNoiseX;
        float finalY = noiseY - initialNoiseY;
        float finalZ = noiseZ - initialNoiseZ;

        transform.localPosition = startLocalPosition + new Vector3(finalX, finalY, finalZ);

        float rotNoiseX = (Mathf.PerlinNoise(Time.time * rotationSpeed + randomOffsetX + 50f, 0f) * 2f - 1f) * rotationAmount;
        float rotNoiseY = (Mathf.PerlinNoise(0f, Time.time * rotationSpeed + randomOffsetY + 50f) * 2f - 1f) * rotationAmount;
        float rotNoiseZ = (Mathf.PerlinNoise(Time.time * rotationSpeed + randomOffsetZ + 50f, Time.time * rotationSpeed + randomOffsetZ + 50f) * 2f - 1f) * rotationAmount;

        float finalRotX = rotNoiseX - initialRotX;
        float finalRotY = rotNoiseY - initialRotY;
        float finalRotZ = rotNoiseZ - initialRotZ;

        transform.localRotation = startLocalRotation * Quaternion.Euler(finalRotX, finalRotY, finalRotZ);
    }
}