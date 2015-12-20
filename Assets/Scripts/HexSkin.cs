using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;

public class HexSkin : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;
    private KnobColor[] knobs;

    public Sprite[] sprites;
    public GameObject knobPrefab;
    public float knobX = 0.2f;
    public float knobY = -0.339f;

    void Awake()
    {
        Debug.Log("HexSkin.Awake");
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;

        knobs = new KnobColor[sprites.Length];
        float xleft = -knobX * (knobs.Length / 2) + (1 - knobs.Length % 2) * (knobX / 2f);
        for (int i = 0; i < knobs.Length; ++i)
        {
            Vector3 pos = new Vector3(xleft + knobX * i, knobY, 0f);
            GameObject knob = Instantiate(knobPrefab);
            knob.transform.SetParent(transform);
            knob.transform.localPosition = pos;
            knobs[i] = knob.GetComponent<KnobColor>();
        }
    }

    void Update()
    {
    }

    public void SetLevel(int lvl)
    {
        HighlightKnobs(lvl);
        lvl = Mathf.Clamp(lvl, -1, sprites.Length - 1);
        if (lvl >= 0)
            spriteRenderer.sprite = sprites[lvl];
        else
            spriteRenderer.sprite = null;
    }

    public void UpdateSortingOrder(Hexagon hexagon)
    {
        OffsetCoord coord = OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, hexagon);
        spriteRenderer.sortingOrder = -(10 * coord.row + (coord.col % 2) * 5);
    }

    private void HighlightKnobs(int count)
    {
        if (count < 0)
            count = 0;
        for (int i = 0; i < count && i < knobs.Length; ++i)
            knobs[i].Highlight();
        for (int i = count; i < knobs.Length; ++i)
            knobs[i].Hide();
    }
}
