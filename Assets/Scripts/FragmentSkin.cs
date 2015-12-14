using UnityEngine;
using System.Collections;

public class FragmentSkin : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sortingOrder = -Mathf.FloorToInt(transform.position.y * 100f);
    }

    void OnBarrierChanged()
    {
    }

}
