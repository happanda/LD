using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class HexSkin : MonoBehaviour
{
    public Sprite[] spritePrefabs;
    private SpriteRenderer spriteRenderer;
    private MovingHex movingHex;
    private Color defaultColor;
    private KnobColor[] knobs;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //var spritePrefab = spritePrefabs[Random.Range(0, spritePrefabs.Length)];
        //renderer.sprite = spritePrefab;
        defaultColor = spriteRenderer.color;
        movingHex = GetComponent<MovingHex>();
        movingHex.levelChanged += OnLevelChanged;

        knobs = new KnobColor[3];
        for (int i = 0; i < 3; ++i)
            knobs[i] = transform.GetChild(i).GetComponent<KnobColor>();
        IslandManager.Inst.barrierChanged += OnBarrierChanged;
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
        if (movingHex.InBarrier())
        {
            spriteRenderer.color = Color.red;
            movingHex.Upgrade();
        }
        else
        {
            spriteRenderer.color = defaultColor;
            movingHex.Downgrade();
        }
    }

    public void HighlightKnobs(int count)
    {
        for (int i = 0; i < count; ++i)
            knobs[i].Highlight();
        for (int i = count; i < knobs.Length; ++i)
            knobs[i].Hide();
    }

    private void OnLevelChanged(int level)
    {
        HighlightKnobs(level);
    }
}
