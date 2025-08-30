using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MapSelector : MonoBehaviour
{
    public int currentMap = 0;

    [SerializeField] private RectTransform mapList;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private float mapWidth;
    [SerializeField] private float margin;
    [SerializeField] private MovementDescr movement;

    public void PopulateMapList()
    {
        // Empty map list
        int childCount = mapList.childCount;
        for (int i = 0; i < childCount; i++) Destroy(mapList.GetChild(i).gameObject);

        foreach (Map map in GameManager.i.maps)
        {
            MapUI ui = Instantiate(mapPrefab, mapList).GetComponent<MapUI>();
            ui.image.sprite = Resources.Load<Sprite>(map.sceneName);
            ui.nameText.text = LocalizationManager.GetValue(map.sceneName);
        }

        SelectMap(currentMap);
    }

    public void Left()
    {
        if (currentMap > 0)
            SelectMap(currentMap - 1);
    }

    public void Right()
    {
        if (currentMap < GameManager.i.maps.Length - 1)
            SelectMap(currentMap + 1);
    }

    public void SelectMap(int i)
    {
        currentMap = i;

        float totalWidth = mapWidth * GameManager.i.maps.Length + margin * (GameManager.i.maps.Length - 1);
        float targetX = totalWidth / 2 - mapWidth / 2 - (mapWidth + margin) * currentMap;
        
        LeanTween.moveLocalX(mapList.gameObject, targetX, movement.duration).setEase(movement.easeType);
    }

    public void Play()
    {
        GameManager.i.Play(currentMap);
    }
}

[Serializable]
public struct Map
{
    public string sceneName;
}

