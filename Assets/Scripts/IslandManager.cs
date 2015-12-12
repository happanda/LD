using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class IslandManager : MonoBehaviour
{
    public GameObject HexPrefab;
        // just a parent for all generated objects
    private Transform boardHolder;
    private Layout layout;
    private HashSet<Hexagon> map = new HashSet<Hexagon>();

    void InitIsland()
    {
        boardHolder = new GameObject("IslandStuff").transform;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size), new Point(0f, 0f));

        const int mapRad = 3;
        for (int q = -mapRad; q <= mapRad; q++)
        {
            int r1 = Math.Max(-mapRad, -q - mapRad);
            int r2 = Math.Min(mapRad, -q + mapRad);
            for (int r = r1; r <= r2; r++)
            {
                Hexagon hex = new Hexagon(q, r, -q - r);
                map.Add(hex);
                Quaternion quat = HexPrefab.transform.rotation;
                Point pnt = Layout.HexagonToPixel(layout, hex);
                GameObject inst = Instantiate(HexPrefab, new Vector3(pnt.x, pnt.y, 0f), quat) as GameObject;
                inst.transform.SetParent(boardHolder);
            }
        }
    }

    void Start()
    {
        InitIsland();
    }

    void Update()
    {
    }
}
