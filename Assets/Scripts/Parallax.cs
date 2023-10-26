using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float amount;

    private void Update()
    {
        Vector3 targetPos = CameraController.i.mainCamera.transform.position * amount;
        targetPos.z = 0;
        transform.position = targetPos;
    }
}
