using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCameraSetup : MonoBehaviour
{
    public int currentProgress = 0; 
    public Vector3[] viewPositions;


    void Start()
    {
        if (viewPositions.Length == 0) return;

        if (currentProgress >= viewPositions.Length)
        {
            currentProgress = viewPositions.Length - 1;
        }

        transform.position = viewPositions[currentProgress];
    }
}