using System.Collections;
using UnityEngine;

public class WindArea : MonoBehaviour
{
    [Header("바람 설정")]
    public Vector3 windDirection = Vector3.left; 
    public float windStrength = 15f; 
    public ForceMode forceMode = ForceMode.Acceleration; 

    [Header("바람 주기 설정")]
    public float windDuration = 3f;
    public float pauseDuration = 2f;

    [Header("시각 효과 (이펙트 & 애니메이션)")]
    public ParticleSystem windParticles;
    
    [Tooltip("선풍기/환풍기 오브젝트의 애니메이터를 연결하세요.")]
    public Animator fanAnimator;

    private bool isWindBlowing = true;

    void Start()
    {
        StartCoroutine(WindCycleRoutine());
    }

    private IEnumerator WindCycleRoutine()
    {
        while (true)
        {
            isWindBlowing = true;
            if (windParticles != null) windParticles.Play();
            
            if (fanAnimator != null) fanAnimator.SetBool("IsBlowing", true);
            
            yield return new WaitForSeconds(windDuration);

            isWindBlowing = false;
            if (windParticles != null) windParticles.Stop();
            
            if (fanAnimator != null) fanAnimator.SetBool("IsBlowing", false);
            
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!isWindBlowing) return;

        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(windDirection.normalized * windStrength, forceMode);
        }
    }
}