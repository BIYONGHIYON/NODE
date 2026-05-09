using UnityEngine;

public class AnchorRenderOrder : MonoBehaviour
{
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.renderQueue = 1999;
        }
    }
}