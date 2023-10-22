using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [NonSerialized] public Player owner;

    [SerializeField] private SpriteRenderer sr;
    public Rigidbody2D rb;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject grapplingPrefab;

    private int contactCount = 0;

    public void Init(Player owner) 
    {
        this.owner = owner;
        sr.color = owner.info.color;
    }

    private void Update()
    {
        if (GameManager.i.CurrentPlayerCharacter == this)
        {
            Util.SetLayerWithChildren(gameObject, LayerMask.NameToLayer("CurrentPlayerCharacter"));
        }
        else
        {
            Util.SetLayerWithChildren(gameObject, LayerMask.NameToLayer("Character"));
        }
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
        rb.AddForce(force, ForceMode2D.Force);
    }

    public void AddImpulse(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void SpawnBomb(Vector2 force)
    {
        GameObject bomb = Instantiate(bombPrefab);
        bomb.transform.position = transform.position;
        bomb.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
    }

    public Grappling SpawnGrappling(Vector2 force, Action callback)
    {
        Grappling grappling = Instantiate(grapplingPrefab).GetComponent<Grappling>();
        grappling.transform.position = transform.position;
        grappling.rb.AddForce(force, ForceMode2D.Impulse);
        grappling.collisionCallback = callback;
        return grappling;
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
