using System.Collections;
using System.Reflection; // [추가됨] 숨겨진 변수를 찾기 위한 기능
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RespawnableObject : MonoBehaviour
{
    [Header("리스폰 연출 설정")]
    public float deathDuration = 0.5f;     
    public float shakeMagnitude = 0.1f;    
    public float respawnDuration = 0.5f;   

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;         
    
    private Rigidbody rb;
    private bool isRespawning = false;     

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale; 
        
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return;
        if (other.CompareTag("DeathZone"))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isRespawning) return;
        if (collision.gameObject.CompareTag("DeathZone"))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    public void ResetToStart()
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private void DisconnectAllRopes()
    {
        RopeAction[] allRopes = FindObjectsOfType<RopeAction>();
        
        foreach (RopeAction rope in allRopes)
        {
            bool isAttached = false;

            SpringJoint[] joints = rope.GetComponents<SpringJoint>();
            foreach (SpringJoint sj in joints)
            {
                if (sj != null && sj.connectedBody == rb)
                {
                    isAttached = true;
                    break;
                }
            }

            if (!isAttached)
            {
                FieldInfo field = typeof(RopeAction).GetField("targetAnchor", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    Transform target = field.GetValue(rope) as Transform;
                    if (target == this.transform)
                    {
                        isAttached = true;
                    }
                }
            }

            if (isAttached)
            {
                rope.SendMessage("DetachAnchor", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        DisconnectAllRopes();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; 

        Vector3 currentPos = transform.position;
        float elapsed = 0f;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathDuration;
            
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = currentPos + new Vector3(offsetX, offsetY, 0);

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);

            yield return null;
        }

        transform.localScale = Vector3.zero;
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        yield return new WaitForSeconds(0.2f); 

        elapsed = 0f;
        while (elapsed < respawnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / respawnDuration;
            
            float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easeProgress);
            
            yield return null;
        }

        transform.localScale = originalScale;
        rb.isKinematic = false;
        isRespawning = false;
    }
}