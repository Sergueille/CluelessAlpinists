using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float amount;

    public Vector2 startPosition;

    private void Start()
    {
        startPosition = gameObject.transform.position;
    }

    private void Update()
    {
        Vector3 targetPos = CameraController.i.mainCamera.transform.position * amount + (Vector3)startPosition;
        targetPos.z = 0;
        transform.position = targetPos;
    }
}
