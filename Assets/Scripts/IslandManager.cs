using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


public class IslandManager : MonoBehaviour
{
    static public IslandManager Inst;

        // set from the Inspector
    public GameObject[] TilePrefabs;
    public GameObject[] FragPrefabs;
    public GameObject EmptyTilePrefab;
    public GameObject MeteorPrefab;

    [HideInInspector]
    public Layout layout;

    public float animationSpeed = 7f;
    public float minSpawnTime = 2.5f;
    public float maxSpawnTime = 4.1f;
    public float minFragSpeed = 0.5f;
    public float maxFragSpeed = 2f;
    public float meteorProbability = 0.07f;

    public delegate void BarrierChanged();
    public event BarrierChanged barrierChanged;


    private IDictionary<Tile, GameObject> tilePrefabs = new Dictionary<Tile, GameObject>();
    private IDictionary<Tile, GameObject> fragPrefabs = new Dictionary<Tile, GameObject>();
    private IDictionary<Tile, int> maxLevels = new Dictionary<Tile, int>();

    private float nextSpawnTime = 0f;


    private Transform islandHolder; // just a parent for all island tiles in Hierarchy window
    private Transform fragmentsHolder; // just a parent for all fragments in Hierarchy window
    [HideInInspector]
    public Transform barrierHolder; // just a parent for all parts of the barrier in Hierarchy window
    private IDictionary<Hexagon, MovableHex> map = new Dictionary<Hexagon, MovableHex>();

    private GroundTile mainHex; // central Tower tile
    private HashSet<Hexagon> barrier = new HashSet<Hexagon>(); // ring of the barrier
    public int barrierRadius
    {
        get;
        private set;
    }
    private bool barrierImproved = false;

    public GameObject[] barrierPrefabs;


    public GameObject GetTilePrefab(Tile type)
    {
        return tilePrefabs[type];
    }

    public GameObject GetFragPrefab(Tile type)
    {
        return fragPrefabs[type];
    }

    private void InitData()
    {
        foreach (var tp in TilePrefabs)
        {
            Tile type = tp.GetComponent<GroundTile>().Type;
            tilePrefabs[type] = tp;
            var hexSkin = tp.GetComponent<HexSkin>();
            maxLevels[type] = hexSkin.sprites.Length;
        }
        foreach (var fp in FragPrefabs)
            fragPrefabs[fp.GetComponent<MovingFragment>().Type] = fp;

        islandHolder = new GameObject("IslandStuff").transform;
        fragmentsHolder = new GameObject("FragmentsStuff").transform;
        barrierHolder = new GameObject("BarrierStuff").transform;
        barrierHolder.transform.position = Vector3.zero;
        barrierHolder.transform.localScale = Vector3.one;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size * Hex.Yscale), new Point(0f, 0f));
    }

    private void InitIsland()
    {
        map.Clear();
        barrier.Clear();
        mainHex = null;
        barrierRadius = -1;
        barrierImproved = false;

        CreateTile(new Hexagon(0, 0), Tile.Main);
        CreateTile(new Hexagon(1, -1), TileExt.Random());
        CreateTile(new Hexagon(-1, 1), TileExt.Random());
        CreateEmptyTile(new Hexagon(0, 1));
        CreateEmptyTile(new Hexagon(0, -1));
        CreateEmptyTile(new Hexagon(1, 0));
        CreateEmptyTile(new Hexagon(-1, 0));
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

        InitData();
        InitIsland();
        nextSpawnTime = 2f;
    }

    void Update()
    {
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

        if (nextSpawnTime < Time.time)
        {
            // TODO: debug
            //if (barrierImproved && Random.value < meteorProbability)
            //    SpawnMeteor();
            //else
                SpawnFragment();
            nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
        }
    }

    private void Turn(bool left)
    {
        var mapCopy = map;
        map = new Dictionary<Hexagon, MovableHex>();
        foreach (var mh in mapCopy)
        {
            mh.Value.Turn(left);
            map[mh.Value.hexagon] = mh.Value;
        }
    }

    public GroundTile CreateTile(Hexagon hex, Tile type)
    {
        if (map.ContainsKey(hex))
        {
            Debug.LogError("CreateTile in place of existing tile: " + hex.ToString());
            return null;
        }
        GameObject hexPrefab = tilePrefabs[type];
        GameObject inst = Instantiate(hexPrefab, Layout.HexagonToPixel(layout, hex), hexPrefab.transform.rotation) as GameObject;
        inst.transform.SetParent(islandHolder);

        MovableHex mh = inst.GetComponent<MovableHex>();
        mh.hexagon = hex;
        map[hex] = mh;

        GroundTile gt = inst.GetComponent<GroundTile>();
        gt.Type = type;
        if (hex.q == 0 && hex.r == 0)
            mainHex = gt;
        //ExpandBarrier();
        return gt;
    }

    public EmptyHex CreateEmptyTile(Hexagon hex)
    {
        if (map.ContainsKey(hex))
        {
            Debug.LogError("CreateTile in place of existing tile: " + hex.ToString());
            return null;
        }
        GameObject inst = Instantiate(EmptyTilePrefab, Layout.HexagonToPixel(layout, hex), EmptyTilePrefab.transform.rotation) as GameObject;
        inst.transform.SetParent(islandHolder);

        MovableHex mh = inst.GetComponent<MovableHex>();
        mh.hexagon = hex;
        map[hex] = mh;
        return inst.GetComponent<EmptyHex>();
    }

    // called from external code: Erase + ShrinkBarrier
    public void Remove(Hexagon hex)
    {
        if (hex.q != 0 || hex.r != 0)
        {
            bool needShrink = false;// InBarrier(hex);
            MovableHex mh = map[hex];
            map.Remove(hex);
            Destroy(mh.gameObject);
            if (needShrink)
                ShrinkBarrier();
        }
        else
        {
            GameOver();
        }
    }

    public bool InBarrier(Hexagon hex)
    {
        return barrier.Contains(hex);
    }

    public int MaxLevel(Tile type)
    {
        return maxLevels[type];
    }

    private bool ExpandBarrier()
    {
        return false;
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
        return false;
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
            //if (map.ContainsKey(h))
            //    map[h].GetComponent<HexSkin>().ClearBarrier();
        }
    }

    private void DrawBarrier(IList<Hexagon> ring)
    {
        Queue<Dir> barDirs = new Queue<Dir>();
        barDirs.Enqueue(Dir.RD);
        barDirs.Enqueue(Dir.RU);
        barDirs.Enqueue(Dir.U);

        if (ring.Count == 1)
        {
            barDirs.Enqueue(Dir.LU);
            barDirs.Enqueue(Dir.LD);
            barDirs.Enqueue(Dir.D);
            HexSkin skin = map[ring[0]].GetComponent<HexSkin>();
            //skin.DrawBarrier(barDirs);
            return;
        }
        
        int idx = ring.Count - 1;
        for (int i = 0; i < 6; ++i)
        {
            HexSkin skin = map[ring[idx++]].GetComponent<HexSkin>();
            //skin.DrawBarrier(barDirs);
            barDirs.Dequeue();
            idx = idx % ring.Count;
            for (int j = 0; j < barrierRadius - 1; ++j)
            {
                skin = map[ring[idx++]].GetComponent<HexSkin>();
                //skin.DrawBarrier(barDirs);
            }
            barDirs.Enqueue((Dir)((i + 2) % 6));
            idx = idx % ring.Count;
        }
    }

    //public void DrawBarrier(IEnumerable<Dir> dir)
    //{
    //    foreach (var d in dir)
    //    {
    //        GameObject prefab = IslandManager.Inst.barrierPrefabs[(int)d];
    //        GameObject bar = Instantiate(prefab) as GameObject;
    //        Vector3 pos = transform.position + prefab.transform.position;
    //        bar.transform.SetParent(IslandManager.Inst.barrierHolder);
    //        bar.transform.localScale = Vector3.one;
    //        bar.transform.position = pos;
    //        if (d == Dir.D || d == Dir.LD || d == Dir.RD)
    //            bar.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder + 50;
    //        else
    //            bar.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder - 50;
    //    }
    //}

    //public void ClearBarrier()
    //{
    //    foreach (Transform child in IslandManager.Inst.barrierHolder)
    //    {
    //        Destroy(child.gameObject);
    //    }
    //}


    // erase everything over the barrier
    private void EraseOverTheBarrier()
    {
        IList<Hexagon> toErase = new List<Hexagon>();
        //foreach(var h in hexes)
        //{
        //    if (Hexagon.Length(h.hexagon) > barrierRadius)
        //        toErase.Add(h.hexagon);
        //}
        foreach (var h in toErase)
            Remove(h);
    }

    private void SpawnFragment()
    {
        GameObject fragPrefab = fragPrefabs[TileExt.Random()];
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

    public void LevelFinished()
    {
        if (Application.loadedLevelName == "TestScene")
        {
            Application.LoadLevel("Level2");
        }
        if (Application.loadedLevelName == "Level2")
        {
            Application.LoadLevel("Level3");
        }
        else
        {
            Application.Quit();
        }
        Debug.Log("LEVEL FINISHED");
    }

}
