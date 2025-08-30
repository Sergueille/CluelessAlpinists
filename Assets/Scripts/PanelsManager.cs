using System.Collections.Generic;
using UnityEngine;

public class PanelsManager : MonoBehaviour
{
    public static PanelsManager i;

    [SerializeField] private RectTransform[] panels;
    [SerializeField] private MovementDescr transition;
    [SerializeField] private RectTransform startPanel;

    [SerializeField] private MovementDescr tutorialBackgroundAppearMovement;
    [SerializeField] private MovementDescr tutorialBackgroundDisappearMovement;
    [SerializeField] private Transform tutorialBackground;

    private RectTransform currentPanel;

    private void Awake()
    {
        i = this;

        foreach (RectTransform panel in panels)
        {
            panel.gameObject.SetActive(false);
        }

        startPanel.gameObject.SetActive(true);
        currentPanel = startPanel;
    }

    public void SelectPanel(RectTransform panel)
    {
        if (currentPanel != null)
        {
            RectTransform oldPanel = currentPanel;
            transition.DoNormalized(t =>
            {
                oldPanel.anchorMin = new Vector2(0, t);
                oldPanel.anchorMax = new Vector2(1, 1 + t);
            }).setOnComplete(() => oldPanel.gameObject.SetActive(false));
        }

        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            transition.DoNormalized(t =>
            {
                panel.anchorMin = new Vector2(0, t - 1);
                panel.anchorMax = new Vector2(1, t);
            });
        }

        currentPanel = panel;
    }


    public void HidePanel()
    {
        SelectPanel(null);
    }

    public void SelectStartPanel()
    {
        SelectPanel(startPanel);
    }

    public void ShowTutoBackground()
    {
        SoundManager.PlaySound("tuto_in");
        tutorialBackgroundAppearMovement.DoReverse(t => tutorialBackground.position = new Vector3(t, 0, 0));
    }

    public void HideTutoBackground()
    {
        SoundManager.PlaySound("tuto_out");
        tutorialBackgroundDisappearMovement.Do(t => tutorialBackground.position = new Vector3(t, 0, 0));
    }
}
