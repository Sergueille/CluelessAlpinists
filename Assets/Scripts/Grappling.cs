using System.Collections.Generic;
using UnityEngine;
using System;

public class Grappling : MonoBehaviour
{
    public Transform owner;
    public Rigidbody2D rb;
    [SerializeField] private Collider2D grapplingCollider;

    public Action collisionCallback;

    [SerializeField] private Transform spriteTransform;
    [SerializeField] private LineRenderer rope;

    private bool attached = false;
    private Transform attachParent = null;
    private Vector2 relativePosition;

    private Vector2 lastPosition;

    private MovementDescr ropeDisappear;
    private bool ropeDisappeared = false;

    public void RemoveRope()
    {
        Color startColor = rope.startColor;
        ropeDisappear.DoNormalized(t => { 
            rope.startColor = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
            rope.endColor = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
        }).setOnComplete(() => ropeDisappeared = true);
    }

    private void Update()
    {
        if (attached)
        {
            transform.position = attachParent.TransformPoint(relativePosition);
        }

        if (!ropeDisappeared)
        {
            rope.positionCount = 2;
            rope.SetPositions(new Vector3[] { transform.InverseTransformPoint(owner.position), Vector3.zero});
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
        collisionCallback();
        SoundManager.PlaySound("grap_hit");
        attached = true;
        attachParent = coll.transform;
        Destroy(rb);
        Destroy(grapplingCollider);
        relativePosition = attachParent.InverseTransformPoint(transform.position);
    }
}
