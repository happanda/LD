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
        shadow = Instantiate(Shadow);
    }

    public void SetSortingOrder(int order)
    {
        spriteRenderer.sortingOrder = order;
        shadow.GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    public void SetVertical(Hexagon hex)
    {
        shadow.transform.position = Layout.HexagonToPixel(IslandManager.Inst.layout, hex);
    }

    void OnDestroy()
    {
        Destroy(shadow.gameObject);
    }
}
