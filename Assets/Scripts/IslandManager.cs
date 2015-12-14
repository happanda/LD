using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;


public enum Tile
{
    [Description("Tower")]
    Main        = 0,
    [Description("Desert")]
    Desert      = 1,
    [Description("Field")]
    Field       = 2,
    [Description("Forest")]
    Forest      = 3,
    [Description("Hills")]
    Hills       = 4,
    [Description("Mountains")]
    Mountains   = 5,
    [Description("Meteor")]
    Meteor      = 6,
}

public static class TileExt
{
    public static readonly int TilesCount = Enum.GetValues(typeof(Tile)).Length - 1; // don't count meteor
    public static Tile Random()
    {
        return (Tile)UnityEngine.Random.Range(1, TilesCount);
    }
    public static String Str(this Tile type)
    {
        FieldInfo fi = type.GetType().GetField(type.ToString());

        if (null != fi)
        {
            object[] attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs != null && attrs.Length > 0)
                return ((DescriptionAttribute)attrs[0]).Description;
        }
        return "ERROR";
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
    public GameObject MeteorPrefab;

    public float minSpawnTime = 1f;
    public float maxSpawnTime = 1.1f;
    public float minFragSpeed = 2f;
    public float maxFragSpeed = 3f;
    public float meteorProbability = 0.07f;

    private float nextSpawnTime = 0f;

    public TileLevelPair[] maxLevels = new TileLevelPair[TileExt.TilesCount];

    public delegate void BarrierChanged();
    public event BarrierChanged barrierChanged;
    
    private Transform islandHolder; // just a parent for all island tiles in Hierarchy window
    private Transform fragmentsHolder; // just a parent for all fragments in Hierarchy window
    public Transform barrierHolder; // just a parent for all parts of the barrier in Hierarchy window
    private IDictionary<Hexagon, MovingHex> map = new Dictionary<Hexagon, MovingHex>();
    private IList<MovingHex> hexes = new List<MovingHex>(); // list of all active tiles
    [HideInInspector]
    public int maxRadius = 0; // maximum distance of tiles from main

    private MovingHex mainHex; // central Tower tile
    private HashSet<Hexagon> barrier = new HashSet<Hexagon>(); // ring of the barrier
    public int barrierRadius
    {
        get;
        private set;
    }
    private bool barrierImproved = false;

    public GameObject[] barrierPrefabs;


    void InitIsland()
    {
        islandHolder = new GameObject("IslandStuff").transform;
        fragmentsHolder = new GameObject("FragmentsStuff").transform;
        barrierHolder = new GameObject("BarrierStuff").transform;
        barrierHolder.transform.position = Vector3.zero;
        barrierHolder.transform.localScale = Vector3.one;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size * Hex.Yscale), new Point(0f, 0f));
        map.Clear();
        hexes.Clear();
        barrier.Clear();
        mainHex = null;
        barrierRadius = -1;
        barrierImproved = false;

        Attach(new Hexagon(0, 0), Tile.Main);
        Attach(new Hexagon(1, -1), TileExt.Random());
        Attach(new Hexagon(-1, 1), TileExt.Random());
        // DEBUG
        Attach(new Hexagon(0, 1), TileExt.Random());
        Attach(new Hexagon(0, -1), TileExt.Random());
        Attach(new Hexagon(1, 0), TileExt.Random());
        Attach(new Hexagon(-1, 0), TileExt.Random());
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
        else if (Input.GetKeyDown("q"))
        {
            SpawnMeteor();
        }

        if (nextSpawnTime < Time.time)
        {
            if (barrierImproved && Random.value < meteorProbability)
                SpawnMeteor();
            else
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
        inst.transform.SetParent(islandHolder);
        MovingHex mh = inst.GetComponent<MovingHex>();
        map[hex] = mh;
        mh.SetCoordinates(hex.q, hex.r);
        hexes.Add(mh);
        if (hex.q == 0 && hex.r == 0)
            mainHex = mh;
        ExpandBarrier();
    }

    // called from external code: Erase + ShrinkBarrier
    public void Remove(Hexagon hex)
    {
        bool needShrink = InBarrier(hex);
        if (hex != mainHex.hexagon)
        {
            Erase(hex);
            if (needShrink)
                ShrinkBarrier();
        }
        else
        {
            GameOver();
        }
    }

    // internal use: Erasing tile
    private void Erase(Hexagon hex)
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
            RemoveOldBarrier(barrier);
            barrier = new HashSet<Hexagon>(ring);
            barrierRadius = newRad;
            Debug.Log("ExpandBarrier TRUE: " + barrierRadius);
            if (barrierRadius > 0)
                mainHex.Upgrade();
            if (barrierRadius > 0)
                barrierImproved = true;
            if (barrierChanged != null)
                barrierChanged();
            DrawBarrier(ring);
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
        EraseOverTheBarrier();
        RemoveOldBarrier(barrier);
        --barrierRadius;
        IList<Hexagon> ring = Hexagon.Ring(new Hexagon(0, 0), barrierRadius);
        barrier = new HashSet<Hexagon>();
        Debug.Log("ShrinkBarrier TRUE: " + barrierRadius);
        mainHex.Downgrade();
        if (barrierChanged != null)
            barrierChanged();
        DrawBarrier(ring);
        return true;
    }

    private void RemoveOldBarrier(IEnumerable<Hexagon> oldBar)
    {
        foreach (var h in oldBar)
        {
            map[h].GetComponent<HexSkin>().ClearBarrier();
        }
    }

    private void DrawBarrier(IList<Hexagon> ring)
    {
        Queue<Dir> barDirs = new Queue<Dir>();
        barDirs.Enqueue(Dir.RU);
        barDirs.Enqueue(Dir.U);
        barDirs.Enqueue(Dir.LU);

        if (ring.Count == 1)
        {
            barDirs.Enqueue(Dir.LD);
            barDirs.Enqueue(Dir.D);
            barDirs.Enqueue(Dir.RD);
            HexSkin skin = map[ring[0]].GetComponent<HexSkin>();
            skin.DrawBarrier(barDirs);
            return;
        }
        
        int idx = 0;
        for (int i = 0; i < 6; ++i)
        {
            HexSkin skin = map[ring[idx++]].GetComponent<HexSkin>();
            skin.DrawBarrier(barDirs);
            barDirs.Dequeue();
            for (int j = 0; j < barrierRadius - 1; ++j)
            {
                skin = map[ring[idx++]].GetComponent<HexSkin>();
                skin.DrawBarrier(barDirs);
            }
            barDirs.Enqueue((Dir)((i + 3) % 6));
        }
    }

    // erase everything over the barrier
    private void EraseOverTheBarrier()
    {
        IList<Hexagon> toErase = new List<Hexagon>();
        foreach(var h in hexes)
        {
            if (Hexagon.Length(h.hexagon) > barrierRadius)
                toErase.Add(h.hexagon);
        }
        foreach (var h in toErase)
            Erase(h);
    }

    private void SpawnFragment()
    {
        GameObject fragPrefab = FragPrefabs[Random.Range(0, FragPrefabs.Length)];
        Instantiate(fragPrefab).transform.SetParent(fragmentsHolder);
    }

    private void SpawnMeteor()
    {
        Instantiate(MeteorPrefab).transform.SetParent(fragmentsHolder);
    }

    private void GameOver()
    {
        barrierRadius = -1;
        EraseOverTheBarrier();
        Debug.Log("GAME OVER");
    }
}
