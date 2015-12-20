using UnityEngine;
using System.Collections;

public class FragmentSkin : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.sortingOrder = 1000;// -Mathf.FloorToInt(transform.position.y * 10f);
    }

}
