using UnityEngine;
using System.Collections;

public class Meteor : MonoBehaviour
{
    public GameObject Shadow;
    public float outerRadius = 12f; // radius of random start position

    private Hexagon vertical_;
    private Vector3 flightDir;
    private float speed;
    private float rotSpeed = Random.Range(60f, 180f) * (Random.value < 0.5f ? -1 : 1);
    private GameObject shadow;

    public Hexagon Vertical
    {
        get { return vertical_; }
    }

    void Awake()
    {
        transform.position = RandomStart();
        flightDir = new Vector3(0f, -1f, 0f);
        speed = Random.Range(IslandManager.Inst.minFragSpeed, IslandManager.Inst.maxFragSpeed);
    }

    void Update()
    {
        transform.position = transform.position + flightDir * speed * Time.deltaTime;
        // if the fragment flies to far from center, destroy it
        if (transform.position.sqrMagnitude > outerRadius * outerRadius * 2)
            Destroy(gameObject);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 30f));
    }

    public Vector3 RandomStart()
    {
        vertical_ = Hexagon.RandomInCircle(IslandManager.Inst.BarrierRadius + 2);
        Vector3 pnt = Layout.HexagonToPixel(IslandManager.Inst.layout, vertical_);
        pnt.y = outerRadius;
        if (Hexagon.Length(vertical_) <= IslandManager.Inst.BarrierRadius + 1)
        {
            shadow = Instantiate(Shadow);
            shadow.transform.position = Layout.HexagonToPixel(IslandManager.Inst.layout, vertical_);
            shadow.GetComponent<SpriteRenderer>().sortingOrder = Vertical.SortingOrder() + 1;
            shadow.transform.SetParent(this.transform);
        }
        return pnt;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ground")
        {
            MovableHex tile = other.GetComponent<MovableHex>();
            if (tile.hexagon == vertical_)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnDestroy()
    {
        Destroy(shadow.gameObject);
    }
}
