using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public Rigidbody2D rb;
    public float explosionDelay;
    public float explosionForce;
    public float explosionRadius;

    [SerializeField] private ParticleSystem explosionPs;
    [SerializeField] private ParticleSystem trailPs;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Collider2D coll;

    private float startTime;
    private bool exploded = false;

    private void Start()
    {
        startTime = Time.time;
        trailPs.Play();
    }

    private void Update()
    {
        if (!exploded && Time.time > startTime + explosionDelay)
        {
            exploded = true;
            Destroy(sr);
            Destroy(rb);
            Destroy(coll);
            Destroy(trailPs);
            explosionPs.Play(); // GameObject destroyed by particle system

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (Collider2D coll in colliders)
            {
                Rigidbody2D other = coll.attachedRigidbody;
                if (other != null)
                {
                    Vector2 direction = other.transform.position - transform.position;
                    other.AddForce(direction.normalized * explosionForce * (1 - direction.magnitude / explosionRadius), ForceMode2D.Impulse); 
                    // TODO: add force on the nearest point towards the bomb?
                }
            }
        }
    }
}
