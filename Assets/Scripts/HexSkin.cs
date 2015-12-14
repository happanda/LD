﻿using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

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
        defaultColor = spriteRenderer.color;
        movingHex = GetComponent<MovingHex>();
        movingHex.levelChanged += OnLevelChanged;

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
        }
        else
        {
            spriteRenderer.color = defaultColor;
        }
    }

    public void HighlightKnobs(int count)
    {
        for (int i = 0; i < count && i < knobs.Length; ++i)
            knobs[i].Highlight();
        for (int i = count; i < knobs.Length; ++i)
            knobs[i].Hide();
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
        IslandManager.Inst.barrierChanged -= OnBarrierChanged;
        movingHex.levelChanged -= OnLevelChanged;
    }
}
