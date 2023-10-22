using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [NonSerialized] public Player owner;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;

    private int contactCount = 0;

    public void Init(Player owner) 
    {
        this.owner = owner;
        sr.color = owner.info.color;
    }

    public bool IsTouchingGround()
    {
        return contactCount > 0;
    }

    public void DisplayJumpTrajectory(Vector2 force)
    {
        // TODO
    }

    public void AddForce(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        contactCount++;
    }

    private void OnCollisionExit2D(Collision2D coll)
    {
        contactCount--;
    }
}
