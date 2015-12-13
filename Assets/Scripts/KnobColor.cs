using UnityEngine;
using System.Collections;

public class KnobColor : MonoBehaviour
{
    private SpriteRenderer renderer;

    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.color = new Color(1f, 1f, 1f, 0f);
    }

    public Color color
    {
        get { return renderer.color; }
        set { renderer.color = value; }
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
