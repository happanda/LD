using UnityEngine;
using System.Collections;

public class BarrierSkin : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.sortingOrder = -Mathf.FloorToInt(transform.position.y * 100f);
    }
}
