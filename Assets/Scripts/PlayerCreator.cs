using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCreator : MonoBehaviour
{
    public static int activatedCount = 0;

    [SerializeField] private Image skin;
    [SerializeField] private TMP_InputField input;

    [SerializeField] private Color enabledColor;
    [SerializeField] private Color disabledColor;
    [SerializeField] private float colorTransitionDuration;
    [SerializeField] private Image background;
    [SerializeField] private Button continueButton;

    public int id;

    private string prefKey;

    private void Awake()
    {
        activatedCount = 0;
    }

    private void Start()
    {
        prefKey = "PlayerName" + id.ToString();

        skin.sprite = GameManager.i.menuInfos[id].skin;
        background.color = disabledColor;
        continueButton.interactable = false;

        if (PlayerPrefs.HasKey(prefKey))
            SetName(PlayerPrefs.GetString(prefKey));
    }

    public void SetName(string newName)
    {
        GameManager.i.menuInfos[id].name = newName;

        input.SetTextWithoutNotify(newName);

        if (newName == "" && GameManager.i.menuInfos[id].activated)
        {
            GameManager.i.menuInfos[id].activated = false;
            LeanTween.value(gameObject, background.color, disabledColor, colorTransitionDuration).setOnUpdate(c => background.color = c);
            activatedCount--;
        }
        else if (newName != "" && !GameManager.i.menuInfos[id].activated)
        {
            GameManager.i.menuInfos[id].activated = true;
            LeanTween.value(gameObject, background.color, enabledColor, colorTransitionDuration).setOnUpdate(c => background.color = c);
            activatedCount++;
        }

        continueButton.interactable = activatedCount >= 2;

        PlayerPrefs.SetString(prefKey, newName);
    }
}
