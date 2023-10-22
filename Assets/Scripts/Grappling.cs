using System.Collections.Generic;
using UnityEngine;
using System;

public class Grappling : MonoBehaviour
{
    public Rigidbody2D rb;

    public Action collisionCallback;

    private void OnCollisionEnter2D(Collision2D coll) 
    {
        Destroy(rb);
        collisionCallback();
    }
}
