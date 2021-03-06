﻿using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class MovingFragment : MonoBehaviour
{
    public float outerRadius = 12f; // radius of random start position

    private Vector3 flightDir;
    private float speed;
    public Tile type;

    private float rotSpeed = Random.Range(60f, 180f) * (Random.value < 0.5f ? -1 : 1);

    void Awake()
    {
        transform.position = RandomStart();
        flightDir = new Vector3(0f, -1f, 0f);// (Vector3.zero - transform.position).normalized;
        speed = Random.Range(IslandManager.Inst.minFragSpeed, IslandManager.Inst.maxFragSpeed);
        //type = Tile.Forest;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + flightDir * speed * Time.deltaTime;
            // if the fragment flies to far from center, destroy it
        if (transform.position.sqrMagnitude > outerRadius * outerRadius * 2)
            Destroy(gameObject);
        if (type == Tile.Meteor)
            transform.Rotate(Vector3.forward * Time.deltaTime * rotSpeed);
    }

    public Vector3 RandomStart()
    {
        int q = Random.Range(-IslandManager.Inst.barrierRadius - 2, IslandManager.Inst.barrierRadius + 3);
        Point pnt = Layout.HexagonToPixel(IslandManager.Inst.layout, new Hexagon(q, 0));
        return new Vector3(pnt.x, outerRadius);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
