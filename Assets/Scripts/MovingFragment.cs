using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System;

public class MovingFragment : MonoBehaviour
{
    public float outerRadius = 12f; // radius of random start position

    public Tile Type;

    private Hexagon vertical_;
    private Vector3 flightDir;
    private float speed;
    private float rotSpeed = Random.Range(60f, 180f) * (Random.value < 0.5f ? -1 : 1);

    public Hexagon Vertical
    {
        get { return vertical_; }
    }

    void Awake()
    {
        transform.position = RandomStart();
        flightDir = new Vector3(0f, -1f, 0f);
        speed = Random.Range(IslandManager.Inst.minFragSpeed, IslandManager.Inst.maxFragSpeed);
        //type = Tile.Forest;
    }

    void Update()
    {
        //transform.position = transform.position + flightDir * speed * Time.deltaTime;
        //    // if the fragment flies to far from center, destroy it
        //if (transform.position.sqrMagnitude > outerRadius * outerRadius * 2)
        //    Destroy(gameObject);
        //transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 30f));
    }

    public Vector3 RandomStart()
    {
        vertical_ = Hexagon.RandomInCircle(IslandManager.Inst.BarrierRadius + 2);
        int q = Random.Range(-IslandManager.Inst.BarrierRadius - 2, IslandManager.Inst.BarrierRadius + 3);
        Vector3 pnt = Layout.HexagonToPixel(IslandManager.Inst.layout, vertical_);
        return pnt;// new Vector3(pnt.x, outerRadius, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Destroy(gameObject);
    }
}
