using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ActionType type;

    [NonSerialized] public Player owner;

    [NonSerialized] public bool moveOnHover = false;
    [NonSerialized] public bool draggable = false;

    public Action<Card> clickCallback;

    [SerializeField] private Image[] icons;
    [SerializeField] private Image image;
    [SerializeField] private Image darkImage;
    [SerializeField] private MovementDescr hoverMovement;
    [SerializeField] private MovementDescr dragStartMovement;
    [SerializeField] private MovementDescr dragSnapMovement;
    [SerializeField] private MovementDescr darkLightMovement;
    [SerializeField] private MovementDescr lowerMovement;

    private bool hovered = false;
    private bool dragged = false;
    private Vector2 dragStartMousePosition;
    private Vector3 dragStartPosition;
    private int lastDragIndex;

    private bool darkened;


    public void Init(ActionType type)
    {
        this.type = type;

        foreach (Image icon in icons)
        {
            icon.sprite = GameManager.i.itemsSprites[(int)type];
        }

        darkImage.color = new Color(0, 0, 0, 0);
    }

    private void Update()
    {
        if (draggable)
        {
            if (hovered && Input.GetMouseButtonDown(0))
            {
                dragged = true;
                hoverMovement.TryCancel();
                dragStartMovement.TryCancel();
                dragStartMovement.Do(t => image.transform.localScale = Vector3.one * (1 + t));
                dragStartMousePosition = Input.mousePosition;
                dragStartPosition = transform.localPosition;
                lastDragIndex = GetNearestPositionInHand();
            }
            else if (dragged && Input.GetMouseButtonUp(0))
            {
                dragged = false;
                dragStartMovement.TryCancel();
                dragStartMovement.DoReverse(t => image.transform.localScale = Vector3.one * (1 + t));

                // Snap to nearest position
                ChangePositionInHand(GetNearestPositionInHand());
            }
            else if (dragged)
            {
                Vector2 delta = (Vector2)Input.mousePosition - dragStartMousePosition;
                delta.Scale(new Vector2(1.0f / Screen.width, 1.0f / Screen.height));
                delta.Scale(CameraController.i.CanvasSize);

                transform.localPosition = new Vector3(dragStartPosition.x + delta.x, dragStartPosition.y, dragStartPosition.z);

                int index = GetNearestPositionInHand();

                if (index > lastDragIndex)
                {
                    for (int i = lastDragIndex; i < index; i++)
                    {
                        owner.hand[i + 1].ChangePositionInHand(i);
                    }
                }
                else if (index < lastDragIndex)
                {
                    for (int i = index; i < lastDragIndex; i++)
                    {
                        owner.hand[i].ChangePositionInHand(i + 1);
                    }
                }

                if (index != lastDragIndex)
                {
                    owner.hand.RemoveAt(lastDragIndex);
                    owner.hand.Insert(index, this);
                }

                lastDragIndex = index;
            }
        }

        if (hovered && Input.GetMouseButtonDown(0) && clickCallback != null)
        {
            clickCallback(this);
        }
    }

    public void ChangePositionInHand(int targetIndex)
    {
        Vector3 targetPosition = new Vector3(
            GameManager.i.GetHandXPosition(targetIndex, owner.hand.Count, false),
            transform.localPosition.y,
            transform.localPosition.z
        );

        dragSnapMovement.DoMovement(v => transform.localPosition = v, transform.localPosition, targetPosition);
    }

    private int GetNearestPositionInHand()
    {
        for (int i = 0; i < owner.hand.Count; i++)
        {
            float xPosition = GameManager.i.GetHandXPosition(i, owner.hand.Count, false);

            if (xPosition > transform.localPosition.x)
            {
                if (i == 0) return 0;

                float previousPosition = GameManager.i.GetHandXPosition(i - 1, owner.hand.Count, false);

                if (xPosition - transform.localPosition.x > transform.localPosition.x - previousPosition)
                {
                    return i - 1;
                }
                else
                {
                    return i;
                }
            }
        }

        return owner.hand.Count - 1;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (moveOnHover && !hovered)
        {
            hoverMovement.TryCancel();
            hoverMovement.Do(t => image.transform.localPosition = Vector3.up * t);
            transform.SetAsLastSibling();
        }

        if (clickCallback == null && !draggable)
        {
            GameManager.i.cursorNotAllowedOverride = true;
        }

        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (moveOnHover && hovered)
        {
            hoverMovement.TryCancel();
            hoverMovement.DoReverse(t => image.transform.localPosition = Vector3.up * t);
        }

        if (clickCallback == null && !draggable)
        {
            GameManager.i.cursorNotAllowedOverride = false;
        }

        hovered = false;
    }

    public void Dark()
    {
        if (darkened) return;

        darkLightMovement.Do(t => darkImage.color = new Color(0, 0, 0, t));
        darkened = true;
    }

    public void Light()
    {
        if (!darkened) return;

        darkLightMovement.DoReverse(t => darkImage.color = new Color(0, 0, 0, t));
        darkened = false;
    }

    private void OnDestroy()
    {
        hoverMovement.TryCancel();
        darkLightMovement.TryCancel();
    }

    public void Lower(int currentIndexInHand)
    {
        Vector3 targetPosition = new Vector3(
            GameManager.i.GetHandXPosition(currentIndexInHand, GameManager.i.cardsInHand, true),
            GameManager.i.handYPositionLowered,
            0.0f
        );

        lowerMovement.DoMovement(pos => transform.localPosition = pos, transform.localPosition, targetPosition);
    }
}   
