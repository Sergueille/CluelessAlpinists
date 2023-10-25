using System.Collections.Generic;
using UnityEngine;

public enum BonusType
{
    none, plus2, exchange
}

public class Bonus : MonoBehaviour
{
    public BonusType type;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private ParticleSystem ps;

    [SerializeField] private bool rotateSprite;
    [SerializeField] private float rotateSpeed;
    
    [SerializeField] private MovementDescr disappearMovement;

    private void Update()
    {
        if (rotateSprite)
        {
            sr.gameObject.transform.eulerAngles = new Vector3(0, 0, Time.time * rotateSpeed);
        }
    }

    public void Touch()
    {
        ps.Stop();
        disappearMovement.DoNormalized(t => sr.transform.localScale = Vector3.one * (1 - t));
        type = BonusType.none;
    }
}
