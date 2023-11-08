using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public Shader replacementShader;
    public Vector2Int resolution;
    public Material blitMaterial;

    public void Start()
    {
        Destroy(gameObject);
    }
}
