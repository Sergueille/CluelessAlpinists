using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private Image img;

    [SerializeField] private float clickDuration;
    [SerializeField] private float bounceDuration;
    [SerializeField] private float scaleAmount;

    private Vector2 pointerInitialPosition;
    private bool pointerLeftButton;

    private void Bounce()
    {
        LeanTween.cancel(img.gameObject);
        LeanTween.scale(img.gameObject, Vector3.one, bounceDuration).setEaseOutElastic().setIgnoreTimeScale(true);
        SoundManager.PlaySound("button");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        LeanTween.cancel(img.gameObject);
        LeanTween.scale(img.gameObject, new Vector3(1 - scaleAmount / 2, 1 - scaleAmount, 1), clickDuration).setIgnoreTimeScale(true);

        pointerInitialPosition = Input.mousePosition;
        pointerLeftButton = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Bounce();

        if (pointerLeftButton && pointerInitialPosition == (Vector2)Input.mousePosition) // Trigger click even if the poiner is outside (since the button shrunk)
        {
            gameObject.GetComponent<Button>().onClick.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerLeftButton = true;
    }
}
