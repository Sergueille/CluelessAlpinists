using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Character : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [NonSerialized] public Player owner;

    [SerializeField] private SpriteRenderer sr;
    public Rigidbody2D rb;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject invertedBombPrefab;
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
    [SerializeField] private SpriteRenderer eyesWhiteSpriteRenderer;
    [SerializeField] private SpriteRenderer eyesBlackSpriteRenderer;
    [SerializeField] private Sprite eyesOpened;
    [SerializeField] private Sprite eyesClosed;
    [SerializeField] private float eyesWhiteDistance;
    [SerializeField] private float eyesBlackDistance;
    [SerializeField] private float eyesClosedDuration;
    [SerializeField] private float eyesClosedInterval;
    [SerializeField] private float minVelocityForEyesClosed = 1.5f;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private MovementDescr nameTextMovement;
    [SerializeField] private SpriteRenderer turnArrow;
    [SerializeField] private float turnArrowSineAmplitude;
    [SerializeField] private float turnArrowSineFrequency;

    private int contactCount = 0;

    private float closeEyesTime = 0;

    private Vector2 arrowStartPosition;

    public void Init(Player owner) 
    {
        this.owner = owner;
        sr.sprite = owner.info.avatar;
        nameText.text = owner.info.name;
        nameText.gameObject.SetActive(false);

        lineRenderer.gameObject.SetActive(false);
        balloons.color = new Color(1, 1, 1, 0);

        arrowStartPosition = turnArrow.transform.localPosition;
    }

    private void Update()
    {
        if (GameManager.i.CurrentPlayerCharacter == this)
        {
            Util.SetLayerWithChildren(gameObject, LayerMask.NameToLayer("CurrentPlayerCharacter"));
            turnArrow.color = new Color(1, 1, 1, 1);
            turnArrow.transform.position = transform.position + (Vector3)arrowStartPosition + Vector3.up * turnArrowSineAmplitude * Mathf.Sin(Time.time * turnArrowSineFrequency);
            turnArrow.transform.rotation = Quaternion.identity;
        }
        else
        {
            Util.SetLayerWithChildren(gameObject, LayerMask.NameToLayer("Character"));
            turnArrow.color = new Color(1, 1, 1, 0);
        }

        lineRenderer.material.color = new Color(lineRenderer.material.color.r, lineRenderer.material.color.g, lineRenderer.material.color.b, lineRenderer.material.color.a - Time.deltaTime * dashedFadeSpeed);

        balloons.transform.eulerAngles = new Vector3(0, 0, 0);

        // Eyes
        Vector2 pointerDir = GameManager.i.GetPointerDirection(transform.position);
        eyesWhiteSpriteRenderer.transform.position = transform.position + (Vector3)pointerDir * eyesWhiteDistance;
        eyesWhiteSpriteRenderer.transform.rotation = Quaternion.identity;
        eyesBlackSpriteRenderer.transform.position = transform.position + (Vector3)pointerDir * eyesBlackDistance;
        eyesBlackSpriteRenderer.transform.rotation = Quaternion.identity;

        if ((Time.time - closeEyesTime) % eyesClosedInterval < eyesClosedDuration)
        {
            eyesWhiteSpriteRenderer.sprite = eyesClosed;
            eyesBlackSpriteRenderer.enabled = false;
        }
        else
        {
            eyesWhiteSpriteRenderer.sprite = eyesOpened;
            eyesBlackSpriteRenderer.enabled = true;
        }
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

    public void SpawnBomb(Vector2 force, bool inverted)
    {
        GameObject bomb = Instantiate(inverted ? invertedBombPrefab : bombPrefab);
        bomb.transform.position = transform.position;
        bomb.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
    }

    public Grappling SpawnGrappling(Vector2 force, Action callback)
    {
        Grappling grappling = Instantiate(grapplingPrefab).GetComponent<Grappling>();
        grappling.transform.position = transform.position + (Vector3)force.normalized * grapplingStartPosition;
        grappling.rb.AddForce(force, ForceMode2D.Impulse);
        grappling.collisionCallback = callback;
        grappling.owner = transform;
        return grappling;
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        contactCount++;

        if (coll.relativeVelocity.sqrMagnitude > minVelocityForEyesClosed * minVelocityForEyesClosed)
        {
            closeEyesTime = Time.time;
        }
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
            if (owner == GameManager.i.CurrentPlayer)
                GameManager.i.bonusAtEndOfTurn = b.type;

            b.Touch();
        }

        Trigger t = coll.gameObject.GetComponent<Trigger>();
        if (t != null)
        {
            t.triggerEvent.Invoke();
        }

        if (coll.gameObject == MapManager.i.finishTrigger)
        {
            GameManager.i.PlayerFinishesRace(owner);
        }
    }
        
    public void OnPointerEnter(PointerEventData eventData)
    {
        nameText.gameObject.SetActive(true);
        nameTextMovement.Do(t => nameText.fontSize = t);
        
        eyesWhiteSpriteRenderer.sprite = eyesClosed;
        eyesBlackSpriteRenderer.enabled = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        nameTextMovement.DoReverse(t => nameText.fontSize = t).setOnComplete(() => nameText.gameObject.SetActive(false));
        
        eyesWhiteSpriteRenderer.sprite = eyesOpened;
        eyesBlackSpriteRenderer.enabled = true;
    }
}
