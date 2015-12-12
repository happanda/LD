using UnityEngine;
using System.Collections;

public class HexSkin : MonoBehaviour
{
    public GameObject spritePrefab;
    private SpriteRenderer renderer;

    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = spritePrefab.GetComponent<SpriteRenderer>().sprite;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
