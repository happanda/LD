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
    public float minSpawnTime = 4.0f;
    public float maxSpawnTime = 4.1f;
    public float minFragSpeed = 0.2f;
    public float maxFragSpeed = 0.3f;
    public float meteorProbability = 0.07f;

    public delegate void BarrierChanged();
    public event BarrierChanged barrierChanged;


    private IDictionary<Tile, GameObject> tilePrefabs = new Dictionary<Tile, GameObject>();
    private IDictionary<Tile, GameObject> fragPrefabs = new Dictionary<Tile, GameObject>();
    private IDictionary<Tile, int> maxLevels = new Dictionary<Tile, int>();

    private float nextSpawnTime = 0f;


    private Barrier barrier;
    private Transform islandHolder; // just a parent for all island tiles in Hierarchy window
    private Transform fragmentsHolder; // just a parent for all fragments in Hierarchy window
    private IDictionary<Hexagon, MovableHex> map = new Dictionary<Hexagon, MovableHex>();
    private GroundTile mainHex; // central Tower tile


    public int BarrierRadius
    {
        get { return barrier.Radius; }
    }

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

        barrier = GetComponent<Barrier>();
        islandHolder = new GameObject("IslandStuff").transform;
        fragmentsHolder = new GameObject("FragmentsStuff").transform;
        layout = new Layout(Layout.flat, new Point(Hex.Size, Hex.Size * Hex.Yscale), new Point(0f, 0f));
    }

    private void InitIsland()
    {
        map.Clear();
        mainHex = null;

        CreateTile(new Hexagon(0, 0), Tile.Main);
        CreateTile(new Hexagon(1, -1), TileExt.Random());
        CreateTile(new Hexagon(-1, 1), TileExt.Random());
        CreateEmptyTile(new Hexagon(0, 1));
        CreateEmptyTile(new Hexagon(0, -1));
        CreateEmptyTile(new Hexagon(1, 0));
        CreateEmptyTile(new Hexagon(-1, 0));

        // DEBUG:
        //int map_radius = 5;
        //for (int q = -map_radius; q <= map_radius; q++)
        //{
        //    int r1 = Mathf.Max(-map_radius, -q - map_radius);
        //    int r2 = Mathf.Min(map_radius, -q + map_radius);
        //    for (int r = r1; r <= r2; r++)
        //    {
        //        CreateTile(new Hexagon(q, r), TileExt.Random());
        //    }
        //}
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
        barrier.GroundAdded(hex);
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

    public void Remove(Hexagon hex)
    {
        if (hex.q != 0 || hex.r != 0)
        {
            MovableHex mh = map[hex];
            map.Remove(hex);
            GroundTile gt = mh.GetComponent<GroundTile>();
            if (gt != null)
                barrier.GroundRemoved(hex);
            Destroy(mh.gameObject);
        }
        else
        {
            GameOver();
        }
    }

    public int MaxLevel(Tile type)
    {
        return maxLevels[type];
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
