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
public class TileExt
{
    public static readonly int TilesCount = Enum.GetValues(typeof(Tile)).Length;
    public static Tile Random()
    {
        return (Tile)UnityEngine.Random.Range(1, TilesCount);
    }
}

public class IslandManager : MonoBehaviour
{
    static public IslandManager Inst;

    public GameObject[] TilePrefabs;
    public Layout layout;
    public GameObject[] FragPrefabs;

    public delegate void BarrierChanged();
    public event BarrierChanged barrierChanged;

    private Transform boardHolder; // just a parent for all generated objects
    private IDictionary<Hexagon, MovingHex> map = new Dictionary<Hexagon, MovingHex>();
    private IList<MovingHex> hexes = new List<MovingHex>(); // list of all active tiles

    private MovingHex mainHex; // central Tower tile
    private HashSet<Hexagon> barrier = new HashSet<Hexagon>(); // ring of the barrier
    private int barrierRadius = 0;


    void InitIsland()
    {
        boardHolder = new GameObject("IslandStuff").transform;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size * Hex.Yscale), new Point(0f, 0f));
        map.Clear();
        hexes.Clear();
        barrier.Clear();
        mainHex = null;
        barrierRadius = 0;

        Attach(new Hexagon(0, 0), Tile.Main);
        mainHex = map[new Hexagon(0, 0)];
        
        // romboid map
        for (int q = -2; q <= 2; q++)
        {
            for (int r = -2; r <= 2; r++)
            {
                Hexagon hex = new Hexagon(q, r);
                Attach(hex, (Tile)Random.Range(1, (int)TileExt.Random()));
            }
        }
    }

    void InitMain()
    {
        Attach(new Hexagon(0, 0), Tile.Main);
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
        else if (Input.GetKeyDown("space"))
        {
            GenFragment();
        }
    }

    private void Attach(Hexagon hex, Tile type)
    {
        if (map.ContainsKey(hex))
        {
            Debug.LogError("Attach in place of existing tile: " + hex.ToString());
            return;
        }
        Point pnt = Layout.HexagonToPixel(layout, hex);
        GameObject hexPrefab = TilePrefabs[(int)type];
        Quaternion quat = hexPrefab.transform.rotation;
        GameObject inst = Instantiate(hexPrefab, new Vector3(pnt.x, pnt.y, 0f), quat) as GameObject;
        inst.transform.SetParent(boardHolder);
        map[hex] = inst.GetComponent<MovingHex>();
        map[hex].SetCoordinates(hex.q, hex.r);
        map[hex].type = type;
        ExpandBarrier();
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


    private void GenFragment()
    {
        GameObject fragPrefab = FragPrefabs[Random.Range(0, FragPrefabs.Length)];
        Instantiate(fragPrefab);
    }
}
