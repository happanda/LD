using UnityEngine;
using System.Collections;

public class FragmentSkin : MonoBehaviour
{
    public Sprite[] spritePrefabs;
    private SpriteRenderer spriteRenderer;
    private MovingFragment movingFrag;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movingFrag = GetComponent<MovingFragment>();
        //spriteRenderer.sprite = spritePrefabs[(int)movingFrag.type - 1];
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
