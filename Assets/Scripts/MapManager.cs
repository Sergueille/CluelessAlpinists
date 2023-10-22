using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager i;

    public Transform startZone;
    
    private void Awake()
    {
       i = this;
    }
}
