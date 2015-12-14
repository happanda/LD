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

[System.Serializable]
public struct TileLevelPair
{
    public Tile type;
    public int maxLevel;
}


public class IslandManager : MonoBehaviour
{
    static public IslandManager Inst;

    public GameObject[] TilePrefabs;
    [HideInInspector]
    public Layout layout;
    public GameObject[] FragPrefabs;

    public float minSpawnTime = 1f;
    public float maxSpawnTime = 1.1f;
    public float minFragSpeed = 2f;
    public float maxFragSpeed = 3f;
    private float nextSpawnTime = 0f;

    public TileLevelPair[] maxLevels = new TileLevelPair[TileExt.TilesCount];

    public delegate void BarrierChanged();
    public event BarrierChanged barrierChanged;

    private Transform boardHolder; // just a parent for all generated objects
    private IDictionary<Hexagon, MovingHex> map = new Dictionary<Hexagon, MovingHex>();
    private IList<MovingHex> hexes = new List<MovingHex>(); // list of all active tiles
    [HideInInspector]
    public int maxRadius = 0; // maximum distance of tiles from main

    private MovingHex mainHex; // central Tower tile
    private HashSet<Hexagon> barrier = new HashSet<Hexagon>(); // ring of the barrier
    private int barrierRadius = -1;


    void InitIsland()
    {
        boardHolder = new GameObject("IslandStuff").transform;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size * Hex.Yscale), new Point(0f, 0f));
        map.Clear();
        hexes.Clear();
        barrier.Clear();
        mainHex = null;
        barrierRadius = -1;

        Attach(new Hexagon(0, 0), Tile.Main);
        mainHex = map[new Hexagon(0, 0)];
        
        // romboid map
        //for (int q = -2; q <= 2; q++)
        //{
        //    for (int r = -2; r <= 2; r++)
        //    {
        //        Hexagon hex = new Hexagon(q, r);
        //        Attach(hex, (Tile)Random.Range(1, (int)TileExt.Random()));
        //    }
        //}
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
        nextSpawnTime = 2f;
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
            SpawnFragment();
        }

        if (nextSpawnTime < Time.time)
        {
            SpawnFragment();
            nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
        }
    }

    public void Attach(Hexagon hex, Tile type)
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
        MovingHex mh = inst.GetComponent<MovingHex>();
        map[hex] = mh;
        mh.SetCoordinates(hex.q, hex.r);
        hexes.Add(mh);
        maxRadius = Math.Max(maxRadius, Hexagon.Length(hex));

        ExpandBarrier();
    }

    public void Remove(Hexagon hex)
    {
        MovingHex mh = map[hex];
        map.Remove(hex);
        hexes.Remove(mh);
        Destroy(mh.gameObject);
    }

    public bool InBarrier(Hexagon hex)
    {
        return barrier.Contains(hex);
    }

    public int MaxLevel(Tile type)
    {
        Debug.Assert(maxLevels.Length == TileExt.TilesCount, "maxLevels array is not long enough");
        int idx = Array.FindIndex(maxLevels, (p => p.type == type));
        Debug.Assert(idx >= 0, "Type " + type.ToString() + " not found in list of max levels");
        return maxLevels[idx].maxLevel;
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
        if (barrierRadius == 0)
        {
            Debug.Log("ShrinkBarrier FALSE: " + barrierRadius);
            return false;
        }
        --barrierRadius;
        barrier = new HashSet<Hexagon>(Hexagon.Ring(new Hexagon(0, 0), barrierRadius));
        Debug.Log("ShrinkBarrier TRUE: " + barrierRadius);
        if (barrierChanged != null)
            barrierChanged();
        return true;
    }

    private void SpawnFragment()
    {
        GameObject fragPrefab = FragPrefabs[Random.Range(0, FragPrefabs.Length)];
        Instantiate(fragPrefab);
    }
}
