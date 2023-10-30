using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLocator : MonoBehaviour
{
    [SerializeField] private GameObject locatorPrefab;

    [Tooltip("% of the height of the screen")]
    [SerializeField] private float displayMargin;

    [Tooltip("% of the height of the screen")]
    [SerializeField] private float outOfScreenMargin;

    private List<Locator> locators;

    private void Start()
    {
        locators = new List<Locator>();
    }

    private void Update()
    {
        if (!GameManager.i.raceStarted) return;

        for (int i = 0; i < GameManager.i.players.Length; i++)
        {
            Character targetCharacter = GameManager.i.players[i].character;
            
            Vector2 screenPos = CameraController.i.mainCamera.WorldToScreenPoint(targetCharacter.transform.position);
            Vector2 canvasSize = CameraController.i.CanvasSize;
            float realDisplayMargin = displayMargin * canvasSize.y / 100.0f;
            float realScreenMargin = outOfScreenMargin * Screen.height / 100.0f;

            bool outOfScreen = screenPos.x < -realScreenMargin 
                            || screenPos.y < -realScreenMargin
                            || screenPos.x > Screen.width + realScreenMargin
                            || screenPos.y > Screen.height + realScreenMargin;
                            
            Locator l = GetLocatorWithID(i);

            if (outOfScreen && !GameManager.i.players[i].finished)
            {
                screenPos.Scale(new Vector2(canvasSize.x / Screen.width, canvasSize.y / Screen.height)); // Now canvas pos

                Vector2 maxDist = canvasSize / 2 - Vector2.one * realDisplayMargin;
                Vector2 centeredPos = screenPos - canvasSize / 2;

                float targetLength = Mathf.Min(
                    maxDist.x / Mathf.Abs(centeredPos.x) * centeredPos.magnitude,
                    maxDist.y / Mathf.Abs(centeredPos.y) * centeredPos.magnitude
                );

                screenPos = centeredPos.normalized * targetLength + canvasSize / 2;

                if (l == null) // No locator created yet
                {
                    l = CreateLocator(targetCharacter, i);
                }

                l.rt.anchoredPosition = screenPos;
                l.rt.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.down, screenPos - canvasSize / 2));
            }
            else if (l != null)
            {
                RemoveLocator(l);
            }
        }
    }

    private Locator GetLocatorWithID(int id)
    {
        foreach (Locator l in locators)
        {
            if (l.id == id) return l;
        }

        return null;
    }

    private Locator CreateLocator(Character character, int id)
    {
        Locator l = GameObject.Instantiate(locatorPrefab, CameraController.i.canvas.transform).GetComponent<Locator>();
        l.Init(character.owner.info.skin, id);
        locators.Add(l);
        return l;
    }

    private void RemoveLocator(Locator l)
    {
        locators.Remove(l);
        l.Remove();
    }
}
