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

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private Image darkImage;
    [SerializeField] private MovementDescr hoverMovement;
    [SerializeField] private MovementDescr dragStartMovement;
    [SerializeField] private MovementDescr dragSnapMovement;
    [SerializeField] private MovementDescr darkLightMovement;

    private bool hovered = false;
    private bool dragged = false;
    private Vector2 dragStartMousePosition;
    private Vector3 dragStartPosition;
    private int lastDragIndex;


    public void Init(ActionType type)
    {
        this.type = type;
        text.text = type.ToString(); // TEST

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
    }

    public void ChangePositionInHand(int targetIndex)
    {
        Vector3 targetPosition = new Vector3(
            GameManager.i.GetHandXPosition(targetIndex, owner.hand.Count),
            transform.localPosition.y,
            transform.localPosition.z
        );   

        dragSnapMovement.DoMovement(v => transform.localPosition = v, transform.localPosition, targetPosition);
    }

    private int GetNearestPositionInHand()
    {
        for (int i = 0; i < owner.hand.Count; i++)
        {
            float xPosition = GameManager.i.GetHandXPosition(i, owner.hand.Count);

            if (xPosition > transform.localPosition.x) 
            {
                if (i == 0) return 0;

                float previousPosition = GameManager.i.GetHandXPosition(i - 1, owner.hand.Count);

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
        if (moveOnHover) 
        {
            hoverMovement.TryCancel();
            hoverMovement.Do(t => image.transform.localPosition = Vector3.up * t);
            transform.SetAsLastSibling();
        }

        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverMovement.TryCancel();
        hoverMovement.DoReverse(t => image.transform.localPosition = Vector3.up * t);
        hovered = false;
    }

    public void Dark()
    {
        darkLightMovement.Do(t => darkImage.color = new Color(0, 0, 0, t));
    }

    public void Light()
    {
        darkLightMovement.DoReverse(t => darkImage.color = new Color(0, 0, 0, t));
    }

    private void OnDestroy()
    {
        hoverMovement.TryCancel();
    }
}   
