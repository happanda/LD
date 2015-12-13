using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public enum Tile
{
    Main        = 0,
    Desert      = 1,
    Field       = 2,
    Forest      = 3,
    Hills       = 4,
    Mountains   = 5,
}

public class IslandManager : MonoBehaviour
{
    static public IslandManager Inst;

    public GameObject[] TilePrefabs;
    public Layout layout;

    public delegate void BarrierChanged();
    public event BarrierChanged barrierChanged;

    private Transform boardHolder; // just a parent for all generated objects
    private IDictionary<Hexagon, MovingHex> map = new Dictionary<Hexagon, MovingHex>();
    private IList<MovingHex> hexes = new List<MovingHex>();

    private MovingHex mainHex; // central Tower tile
    private HashSet<Hexagon> barrier = new HashSet<Hexagon>();
    private int barrierRadius = 0;


    void InitIsland()
    {
        boardHolder = new GameObject("IslandStuff").transform;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size * Hex.Yscale), new Point(0f, 0f));
        map.Clear();
        hexes.Clear();
        barrier.Clear();
        mainHex = null;

        // romboid map
        for (int q = -3; q <= 3; q++)
        {
            for (int r = -3; r <= 3; r++)
            {
                Hexagon hex = new Hexagon(q, r);
                Point pnt = Layout.HexagonToPixel(layout, hex);
                GameObject hexPrefab = TilePrefabs[Random.Range(0, TilePrefabs.Length)];
                if (q == 0 && r == 0)
                    hexPrefab = TilePrefabs[(int)Tile.Main];
                Quaternion quat = hexPrefab.transform.rotation;
                GameObject inst = Instantiate(hexPrefab, new Vector3(pnt.x, pnt.y, 0f), quat) as GameObject;
                inst.transform.SetParent(boardHolder);
                map[hex] = inst.GetComponent<MovingHex>();
                map[hex].SetCoordinates(q, r);
            }
        }
        mainHex = map[new Hexagon(0, 0)];
        barrier.Add(new Hexagon(0, 0));
        barrierRadius = 0;
        if (barrierChanged != null)
            barrierChanged();
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
        //int horiz = (int)Input.GetAxisRaw("Horizontal");
        //int verti = (int)Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown("left"))
        {
            Turn(true);
        }
        else if (Input.GetKeyDown("right"))
        {
            Turn(false);
        }
        else if (Input.GetKeyDown("up"))
        {
            ExpandBarrier();
        }
        else if (Input.GetKeyDown("down"))
        {
            ShrinkBarrier();
        }
    }

    public void AddMovingHex(MovingHex mh)
    {
        hexes.Add(mh);
    }

    public bool InBarrier(Hexagon hex)
    {
        return barrier.Contains(hex);
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

    private bool ExpandBarrier()
    {
        int newRad = barrierRadius + 1;
        IList<Hexagon> ring = Hexagon.Ring(new Hexagon(0, 0), newRad);
        if (ring.All(item => map.ContainsKey(item)))
        {
            barrier = new HashSet<Hexagon>(ring);
            barrierRadius = newRad;
            Debug.Log("ExpandBarrier TRUE: " + barrierRadius);
            if (barrierChanged != null)
                barrierChanged();
            return true;
        }
        Debug.Log("ExpandBarrier FALSE: " + barrierRadius);
        return false;
    }

    private bool ShrinkBarrier()
    {
        //IList<Hexagon> ring = Hexagon.Ring(new Hexagon(0, 0), barrierRadius);
        //if (ring.All(item => map.ContainsKey(item)))
        //{
        //    Debug.Log("ShrinkBarrier FALSE: " + barrierRadius);
        //    return false;
        //}
        if (barrierRadius == 0)
        {
            Debug.Log("ShrinkBarrier FALSE: " + barrierRadius);
            return false;
        }
        --barrierRadius;
        //Debug.Assert(barrierRadius >= 0, "Barrier radius is less than zero?!");
        barrier = new HashSet<Hexagon>(Hexagon.Ring(new Hexagon(0, 0), barrierRadius));
        Debug.Log("ShrinkBarrier TRUE: " + barrierRadius);
        if (barrierChanged != null)
            barrierChanged();
        return true;
    }
}
