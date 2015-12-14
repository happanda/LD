using UnityEngine;
using System.Collections;

public class BarrierSkin : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public int sortOrder;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //spriteRenderer.sortingOrder = sortOrder;
    }
}
