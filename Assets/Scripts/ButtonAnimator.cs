using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image img;

    [SerializeField] private float clickDuration;
    [SerializeField] private float bounceDuration;
    [SerializeField] private float scaleAmount;

    private void Bounce()
    {
        LeanTween.cancel(img.gameObject);
        LeanTween.scale(img.gameObject, Vector3.one, bounceDuration).setEaseOutElastic();
        SoundManager.PlaySound("button");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        LeanTween.cancel(img.gameObject);
        LeanTween.scale(img.gameObject, new Vector3(1 - scaleAmount / 2, 1 - scaleAmount, 1), clickDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Bounce();
    }
}
