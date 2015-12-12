using UnityEngine;
using System.Collections;

public class MovingHex : MonoBehaviour
{
    private Hexagon hexagon_; // actual position
    private PolygonCollider2D collider;
    private Rigidbody2D rb2D;
    private SpriteRenderer renderer;

    public Hexagon hexagon
    {
        get { return this.hexagon_; }
        set
        {
            hexagon_ = value;
            GetComponent<SpriteRenderer>().sortingOrder = hexagon.q;
        }
    }

    public void SetCoordinates(int q, int r)
    {
        hexagon = new Hexagon(q, r);
    }

    void Awake()
    {
        Debug.Log("MovingHex.Awake");
        collider = GetComponent<PolygonCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        IslandManager.Inst.AddMovingHex(this);
        renderer.color = UnityEngine.Random.ColorHSV();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Point pnt = Layout.HexagonToPixel(IslandManager.Inst.layout, hexagon);
        Vector3 dest = new Vector3(pnt.x, pnt.y, 0f);
        float dist = (transform.position - dest).sqrMagnitude;
        if (dist > float.Epsilon)
        {
            //Vector3 newPos = Vector3.RotateTowards(transform.position, dest, 10f * Time.deltaTime, 0f);
            Vector3 newPos = Vector3.MoveTowards(transform.position, dest, 10f * Time.deltaTime);
            transform.position = newPos;
        }
    }
}
