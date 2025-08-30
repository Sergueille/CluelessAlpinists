
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlatformPrevention : MonoBehaviour
{
    private void Start()
    {
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
    }
}
