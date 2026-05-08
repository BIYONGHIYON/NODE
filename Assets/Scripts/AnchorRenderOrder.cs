using UnityEngine;

public class AnchorRenderOrder : MonoBehaviour
{
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // 일반적인 3D 불투명 오브젝트의 Render Queue 기본값은 2000입니다.
            // 이 값을 1999로 낮춰서, 다른 오브젝트들보다 1순위로 먼저 그려지게 만듭니다.
            rend.material.renderQueue = 1999;
        }
    }
}