using UnityEngine;

public class Moving : MonoBehaviour
{
    [Header("Random Movement Settings")]
    public float moveSpeed = 0.5f;
    public float moveRange = 1.0f;
    public float rotationSpeed = 0.3f;
    public float rotationAmount = 15.0f;

    private Vector3 startPosition; 
    private Quaternion startRotation;

    private float randomOffsetX;
    private float randomOffsetY;
    private float randomOffsetZ;

    void Start()
    {
        startPosition = transform.position; 
        startRotation = transform.rotation;

        randomOffsetX = Random.Range(0f, 100f);
        randomOffsetY = Random.Range(0f, 100f);
        randomOffsetZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        float noiseX = (Mathf.PerlinNoise(Time.time * moveSpeed + randomOffsetX, 0f) * 2f - 1f) * moveRange;
        float noiseY = (Mathf.PerlinNoise(0f, Time.time * moveSpeed + randomOffsetY) * 2f - 1f) * moveRange;
        float noiseZ = (Mathf.PerlinNoise(Time.time * moveSpeed + randomOffsetZ, Time.time * moveSpeed + randomOffsetZ) * 2f - 1f) * moveRange;

        transform.position = startPosition + new Vector3(noiseX, noiseY, noiseZ);

        float rotNoiseX = (Mathf.PerlinNoise(Time.time * rotationSpeed + randomOffsetX + 50f, 0f) * 2f - 1f) * rotationAmount;
        float rotNoiseY = (Mathf.PerlinNoise(0f, Time.time * rotationSpeed + randomOffsetY + 50f) * 2f - 1f) * rotationAmount;
        float rotNoiseZ = (Mathf.PerlinNoise(Time.time * rotationSpeed + randomOffsetZ + 50f, Time.time * rotationSpeed + randomOffsetZ + 50f) * 2f - 1f) * rotationAmount;

        transform.rotation = startRotation * Quaternion.Euler(rotNoiseX, rotNoiseY, rotNoiseZ);
    }
}