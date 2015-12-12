using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class IslandManager : MonoBehaviour
{
    static public IslandManager Inst;

    public GameObject HexPrefab;
    public Layout layout;
        // just a parent for all generated objects
    private Transform boardHolder;
    private IDictionary<Hexagon, MovingHex> map = new Dictionary<Hexagon, MovingHex>();
    private IList<MovingHex> hexes = new List<MovingHex>();

    public void AddMovingHex(MovingHex mh)
    {
        hexes.Add(mh);
    }

    void InitIsland()
    {
        boardHolder = new GameObject("IslandStuff").transform;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size * 0.8f), new Point(0f, 0f));
        map.Clear();
        hexes.Clear();

        // romboid map
        for (int q = -2; q <= 2; q++)
        {
            for (int r = -2; r <= 2; r++)
            {
                Hexagon hex = new Hexagon(q, r);
                Quaternion quat = HexPrefab.transform.rotation;
                Point pnt = Layout.HexagonToPixel(layout, hex);
                GameObject inst = Instantiate(HexPrefab, new Vector3(pnt.x, pnt.y, 0f), quat) as GameObject;
                inst.transform.SetParent(boardHolder);
                map[hex] = inst.GetComponent<MovingHex>();
                map[hex].SetCoordinates(q, r);
            }
        }
    }

    void Awake()
    {
        Debug.Log("IslandManager.Awake");
        if (Inst == null)
            Inst = this;
        else if (Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        InitIsland();
    }

    void Start()
    {
    }

    void Update()
    {
        int horiz = 0;
        int verti = 0;
        horiz = (int)Input.GetAxisRaw("Horizontal");
        verti = (int)Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown("left"))
        {
            Turn(true);
        }
        else if (Input.GetKeyDown("right"))
        {
            Turn(false);
        }
    }

    private void Turn(bool left)
    {
        Debug.Log("Turn " + (left ? "Left" : "Right"));
        map.Clear();
        foreach (var hex in hexes)
        {
            Hexagon newHex = hex.hexagon.Rotate(left);
            map[newHex] = hex;
            map[newHex].hexagon = newHex;
        }
    }
}
