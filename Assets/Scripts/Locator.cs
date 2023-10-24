using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Locator : MonoBehaviour
{
    public Image icon;
    public RectTransform rt;

    [NonSerialized] public int id;

    [SerializeField] private MovementDescr sizeMovement;

    public void Init(Sprite sprite, int id)
    {
        icon.sprite = sprite;
        this.id = id;
        sizeMovement.Do(t => transform.localScale = Vector3.one * t);
    }

    public void Remove()
    {
        sizeMovement.DoReverse(t => transform.localScale = Vector3.one * t).setOnComplete(() => Destroy(gameObject));
    }
}
