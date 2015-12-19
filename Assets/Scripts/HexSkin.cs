﻿using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;

public class HexSkin : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MovingHex movingHex;
    private Color defaultColor;
    private KnobColor[] knobs;

    public Sprite[] sprites;
    public GameObject knobPrefab;
    public float knobX = 0.2f;
    public float knobY = -0.339f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movingHex = GetComponent<MovingHex>();
        movingHex.levelChanged += OnLevelChanged;
        defaultColor = spriteRenderer.color;
        UpdateSortinOrder();

        knobs = new KnobColor[IslandManager.Inst.MaxLevel(movingHex.type)];
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
        UpdateSortinOrder();
    }

    private void UpdateSortinOrder()
    {
        OffsetCoord coord = OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, movingHex.hexagon);
        spriteRenderer.sortingOrder = 10 * coord.row + (coord.col % 2) * 5;
    }

    public void HighlightKnobs(int count)
    {
        for (int i = 0; i < count && i < knobs.Length; ++i)
            knobs[i].Highlight();
        for (int i = count; i < knobs.Length; ++i)
            knobs[i].Hide();
    }

    public void DrawBarrier(IEnumerable<Dir> dir)
    {
        foreach (var d in dir)
        {
            GameObject prefab = IslandManager.Inst.barrierPrefabs[(int)d];
            GameObject bar = Instantiate(prefab) as GameObject;
            Vector3 pos = transform.position + prefab.transform.position;
            bar.transform.SetParent(IslandManager.Inst.barrierHolder);
            bar.transform.localScale = Vector3.one;
            bar.transform.position = pos;
            if (d == Dir.D || d == Dir.LD || d == Dir.RD)
                bar.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder + 50;
            else
                bar.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder - 50;
        }
    }

    public void ClearBarrier()
    {
        foreach (Transform child in IslandManager.Inst.barrierHolder)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnLevelChanged(int level)
    {
        HighlightKnobs(level);
        --level;
        if (level < 0)
            level = 0;
        if (sprites.Length > level)
            spriteRenderer.sprite = sprites[level];
    }

    void OnDestroy()
    {
        movingHex.levelChanged -= OnLevelChanged;
    }
}
