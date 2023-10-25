using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager i;

    public Transform startZone;

    public GameObject finishTrigger;
    public ParticleSystem finishParticles;
    
    private void Awake()
    {
       i = this;
    }
}
