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
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float linePointsDeltaTime = 0.1f;
    [SerializeField] private int linePointsCount = 150;
    [SerializeField] private float dashedLineOffsetSpeed = 0.5f;
    [SerializeField] private float dashedFadeSpeed = 0.5f;
    [SerializeField] private float grapplingStartPosition = 0.5f;
    [SerializeField] private SpriteRenderer balloons;
    [SerializeField] private MovementDescr balloonsScaleMovement;
    [SerializeField] private ParticleSystem balloonsParticles;
    [SerializeField] private ParticleSystem jetpackParticles;

    private int contactCount = 0;

    public void Init(Player owner) 
    {
        this.owner = owner;
        sr.sprite = owner.info.avatar;
        lineRenderer.gameObject.SetActive(false);
        balloons.color = new Color(1, 1, 1, 0);
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

        lineRenderer.material.color = new Color(lineRenderer.material.color.r, lineRenderer.material.color.g, lineRenderer.material.color.b, lineRenderer.material.color.a - Time.deltaTime * dashedFadeSpeed);

        balloons.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public bool IsTouchingGround()
    {
        return contactCount > 0;
    }

    public void DisplayJumpTrajectory(Vector2 force)
    {
        Vector3[] positions = new Vector3[linePointsCount];
        Vector2 velocity = force;
        Vector2 pos = transform.position;
        positions[0] = pos;
        for (int i = 1; i < linePointsCount; i++)
        {
            velocity += Physics2D.gravity * linePointsDeltaTime;
            pos += velocity * linePointsDeltaTime;
            positions[i] = new Vector3(pos.x, pos.y, 0);
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
        lineRenderer.gameObject.SetActive(true);
        lineRenderer.material.color = new Color(lineRenderer.material.color.r, lineRenderer.material.color.g, lineRenderer.material.color.b, 1);
        lineRenderer.material.SetVector("_MainTex_ST", new Vector4(1.4f, 1, Time.time * dashedLineOffsetSpeed, 0));
    }

    public void ShowBalloons()
    {
        balloons.color = new Color(1, 1, 1, 1);
        balloonsScaleMovement.Do(t => balloons.transform.localScale = Vector3.one * t);
    }

    public void HideBalloons()
    {
        balloons.color = new Color(1, 1, 1, 0);
        balloonsParticles.Play();
    }

    public void AddForce(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Force);
    }

    public void AddImpulse(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void ToggleJetpackParticles(bool enabled, Vector2 direction)
    {
        if (enabled && !jetpackParticles.isPlaying)
            jetpackParticles.Play();
        else if (!enabled && jetpackParticles.isPlaying)
            jetpackParticles.Stop();

        if (enabled)
            jetpackParticles.transform.eulerAngles = new Vector3(Vector2.SignedAngle(Vector2.right, direction), -90, -90);
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
        grappling.transform.position = transform.position + (Vector3)force.normalized * grapplingStartPosition;
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

    private void OnTriggerEnter2D(Collider2D coll)
    {
        Bonus b = coll.gameObject.GetComponent<Bonus>();
        if (b != null && b.type != BonusType.none)
        {
            GameManager.i.bonusAtEndOfTurn = b.type;
            b.Touch();
        }
    }
}
