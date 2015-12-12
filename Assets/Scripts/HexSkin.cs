using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class HexSkin : MonoBehaviour
{
    public Sprite[] spritePrefabs;
    private SpriteRenderer renderer;

    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        var spritePrefab = spritePrefabs[Random.Range(0, spritePrefabs.Length)];
        renderer.sprite = spritePrefab;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        renderer.sortingOrder = -Mathf.FloorToInt(transform.position.y * 100f);
    }
}
