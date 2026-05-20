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

    // ==========================================
    [Header("시각 효과 (이펙트 & 애니메이션)")]
    public ParticleSystem windParticles;
    
    [Tooltip("선풍기/환풍기 오브젝트의 애니메이터를 연결하세요.")]
    public Animator fanAnimator; // [추가됨] 선풍기 애니메이터 제어용
    // ==========================================

    private bool isWindBlowing = true;

    void Start()
    {
        StartCoroutine(WindCycleRoutine());
    }

    private IEnumerator WindCycleRoutine()
    {
        while (true)
        {
            // 1. 바람 켜기 (선풍기 가동!)
            isWindBlowing = true;
            if (windParticles != null) windParticles.Play();
            
            // 애니메이터의 "IsBlowing" 파라미터를 true로 만들어 날개를 회전시킵니다.
            if (fanAnimator != null) fanAnimator.SetBool("IsBlowing", true);
            
            yield return new WaitForSeconds(windDuration);

            // 2. 바람 끄기 (선풍기 정지!)
            isWindBlowing = false;
            if (windParticles != null) windParticles.Stop();
            
            // 애니메이터의 "IsBlowing" 파라미터를 false로 만들어 날개를 멈춥니다.
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