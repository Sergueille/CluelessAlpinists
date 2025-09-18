using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MapUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image image;
    public TextMeshProUGUI difficultyText;

    public void SetDifficultyText(Difficulty diff)
    {
        difficultyText.text = LocalizationManager.currentDict[diff switch
        {
            Difficulty.Hard => "diff_hard",
            Difficulty.VeryHard => "diff_very_hard",
            Difficulty.ExtremelyHard => "diff_extremely_hard",
            _ => throw new NotImplementedException()
        }];
    }
}
