using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController i;

    public Camera mainCamera;
    public Canvas canvas;

    [SerializeField] private Vector2 offset;
    [SerializeField] private float cameraSpeed;

    private Vector2 cameraVelocity;
    private float startZ;

    private RectTransform canvasTransform;

    public Vector2 CanvasSize
    {
        get => canvasTransform.sizeDelta;
    }
    
    private void Awake()
    {
       i = this;
       startZ = transform.position.z;
       canvasTransform = canvas.gameObject.GetComponent<RectTransform>();
    }    

    private void Update()
    {
        Vector2 targetPosition = GameManager.i.CurrentPlayerCharacter.transform.position;
        Vector2 finalPosition = targetPosition + offset * mainCamera.orthographicSize;

        Vector2 smoothedPos = Vector2.SmoothDamp(transform.position, finalPosition, ref cameraVelocity, cameraSpeed);
        transform.position = new Vector3(smoothedPos.x, smoothedPos.y, startZ);
    }
}
