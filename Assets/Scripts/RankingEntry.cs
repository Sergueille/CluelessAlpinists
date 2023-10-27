using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingEntry : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private Image skin;

    [SerializeField] private MovementDescr bgAppear;
    [SerializeField] private MovementDescr skinAppear;

    
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI turnsText;

    [SerializeField] private Color[] rankColors;

    public void Init(Player player)
    {
        int rank = player.rank == -1 ? GameManager.i.PlayerCount : player.rank;
        rankText.text = "#" + rank.ToString();
        rankText.color = rankColors[rank - 1];

        nameText.text = player.info.name;

        if (player.rank == -1)
        {
            turnsText.text = "Pas fini";
        }
        else
        {
            turnsText.text = player.turns.ToString() + " tours";
        }

        skin.sprite = player.info.skin;

        background.anchorMax = new Vector2(0, 1);
        skin.transform.localScale = Vector3.zero;
        bgAppear.DoNormalized(t => background.anchorMax = new Vector2(t, 1));
        skinAppear.DoNormalized(t => skin.transform.localScale = Vector3.one * t);
    }
}
