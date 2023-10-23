using System.Collections.Generic;
using UnityEngine;
using System;

public class Grappling : MonoBehaviour
{
    public Rigidbody2D rb;

    public Action collisionCallback;

    [SerializeField] private Transform spriteTransform;

    private bool attached = false;
    private Transform parent = null;
    private Vector2 relativePosition;

    private Vector2 lastPosition;

    private void Update()
    {
        if (attached)
        {
            transform.position = parent.TransformPoint(relativePosition);
        }
    }

    private void FixedUpdate()
    {
        if (!attached)
        {
            float angle = Vector2.SignedAngle(Vector2.up, (Vector2)transform.position - lastPosition);
            spriteTransform.eulerAngles = new Vector3(0, 0, angle);
        }
        
        lastPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D coll) 
    {
        Destroy(rb);
        collisionCallback();
        attached = true;
        parent = coll.transform;
        relativePosition = parent.InverseTransformPoint(transform.position);
    }
}
