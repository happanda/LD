using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class HexSkin : MonoBehaviour
{
    
    public Sprite[] spritePrefabs;
    private SpriteRenderer renderer;
    private MovingHex movingHex;
    private Color defaultColor;

    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        //var spritePrefab = spritePrefabs[Random.Range(0, spritePrefabs.Length)];
        //renderer.sprite = spritePrefab;
        defaultColor = renderer.color;
        movingHex = GetComponent<MovingHex>();
        IslandManager.Inst.barrierChanged += OnBarrierChanged;
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

    void OnBarrierChanged()
    {
        if (movingHex.InBarrier())
            renderer.color = Color.red;
        else
            renderer.color = defaultColor;
    }
}
