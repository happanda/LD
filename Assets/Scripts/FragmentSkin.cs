using UnityEngine;
using System.Collections;

public class FragmentSkin : MonoBehaviour
{
    public GameObject Shadow;

    private SpriteRenderer spriteRenderer;
    private MovingFragment moveFrag;
    private GameObject shadow;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveFrag = GetComponent<MovingFragment>();
    }

    public void SetSortingOrder(int order)
    {
        spriteRenderer.sortingOrder = order;
        if (shadow != null)
            shadow.GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    public void SetVertical(Hexagon hex)
    {
        if (Hexagon.Length(hex) <= IslandManager.Inst.BarrierRadius + 1)
        {
            shadow = Instantiate(Shadow);
            shadow.transform.position = Layout.HexagonToPixel(IslandManager.Inst.layout, hex) + shadow.transform.localPosition;
        }
    }

    void OnDestroy()
    {
        if (shadow != null)
            Destroy(shadow.gameObject);
    }
}
