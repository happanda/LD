using UnityEngine;
using System.Collections;

public class KnobColor : MonoBehaviour
{
    public Color highlight = Color.green;
    public Color ordinary = Color.clear;
    public float colorTransitTime = 1.0f;

    private SpriteRenderer spriteRenderer;
    private Color desiredColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = ordinary;
    }

    public void Highlight()
    {
        color = highlight;
    }

    public void Hide()
    {
        color = ordinary;
    }

    public Color color
    {
        get { return spriteRenderer.color; }
        set
        {
            desiredColor = value;
            if (desiredColor != ordinary)
                StartCoroutine(TransitColor());
            else
                spriteRenderer.color = desiredColor;
        }
    }

    private IEnumerator TransitColor()
    {
        float elapsed = 0f;
        while (spriteRenderer.color != desiredColor)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, desiredColor, elapsed / colorTransitTime);
            yield return null;
        }
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
