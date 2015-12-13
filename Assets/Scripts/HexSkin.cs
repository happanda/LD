using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class HexSkin : MonoBehaviour
{
    
    public Sprite[] spritePrefabs;
    private SpriteRenderer renderer;
    private MovingHex movingHex;
    private Color defaultColor;
    private KnobColor[] knobs;

    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        //var spritePrefab = spritePrefabs[Random.Range(0, spritePrefabs.Length)];
        //renderer.sprite = spritePrefab;
        defaultColor = renderer.color;
        movingHex = GetComponent<MovingHex>();

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
        renderer.sortingOrder = -Mathf.FloorToInt(transform.position.y * 100f);
    }

    void OnBarrierChanged()
    {
        if (movingHex.InBarrier())
        {
            renderer.color = Color.red;
            HighlightKnob(0);
            HighlightKnob(2);
        }
        else
        {
            renderer.color = defaultColor;
            HideKnob(0);
            HideKnob(2);
        }
    }

    public void HighlightKnob(int idx)
    {
        knobs[idx].Highlight();
    }

    public void HideKnob(int idx)
    {
        knobs[idx].Hide();
    }
}
