using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager i;

    public Transform startZone;

    public GameObject finishTrigger;
    public ParticleSystem finishParticles;

    [Tooltip("Positive push towards right")]
    public float windForce = 0;
    
    private void Awake()
    {
       i = this;
    }
}
