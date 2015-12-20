using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Barrier : MonoBehaviour
{
    public GameObject[] BarrierPrefabs;

    public int Radius = 0;

    private Transform barrierHolder; // hold the barrier stuff
    private IDictionary<int, int> ground = new Dictionary<int, int>(); // number of tiles by length (distance to center)
    

    void Awake()
    {
        barrierHolder = new GameObject("BarrierStuff").transform;
        barrierHolder.transform.position = Vector3.zero;
        barrierHolder.transform.localScale = Vector3.one;
        Radius = 0;
    }

    public void GroundAdded(Hexagon hex)
    {
        int dist = Hexagon.Length(hex);
        if (!ground.ContainsKey(dist))
            ground[dist] = 0;
        ++ground[dist];
        if (dist == 0 || ground[dist] == dist * 6)
        {
            Radius = dist;
            UpdateBarrier();
        }
    }

    public void GroundRemoved(Hexagon hex)
    {
        int dist = Hexagon.Length(hex);
        Debug.Assert(dist == ground.Count - 1, "Removed hex was not in barrier cache");
        --ground[dist];
        if (ground[dist] == 0)
            ground.Remove(dist);
        if (dist == Radius)
        {
            --Radius;
            UpdateBarrier();
        }
    }

    private void UpdateBarrier()
    {
        if (ground.Count == 0)
            return;

        IList<Hexagon> ring = Hexagon.Ring(new Hexagon(0, 0), Radius);
        RemoveOldBarrier();
        DrawBarrier(ring);
    }

    private void RemoveOldBarrier()
    {
        foreach (Transform child in barrierHolder)
            Destroy(child.gameObject);
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
            DrawBarrier(ring[0], barDirs);
            return;
        }

        int idx = ring.Count - 1;
        for (int i = 0; i < 6; ++i)
        {
            DrawBarrier(ring[idx++], barDirs);
            barDirs.Dequeue();
            idx = idx % ring.Count;
            for (int j = 0; j < Radius - 1; ++j)
            {
                DrawBarrier(ring[idx++], barDirs);
            }
            barDirs.Enqueue((Dir)((i + 2) % 6));
            idx = idx % ring.Count;
        }
    }

    private void DrawBarrier(Hexagon hex, IEnumerable<Dir> dir)
    {
        foreach (var d in dir)
        {
            GameObject prefab = BarrierPrefabs[(int)d];
            GameObject bar = Instantiate(prefab) as GameObject;
            Vector3 pos = Layout.HexagonToPixel(IslandManager.Inst.layout, hex) + prefab.transform.position;
            bar.transform.SetParent(barrierHolder);
            bar.transform.localScale = Vector3.one;
            bar.transform.position = pos;

            bar.GetComponent<SpriteRenderer>().sortingOrder = 500;

            //if (d == Dir.D || d == Dir.LD || d == Dir.RD)
            //    bar.GetComponent<SpriteRenderer>().sortingOrder = hex.sortingOrder + 50;
            //else
            //    bar.GetComponent<SpriteRenderer>().sortingOrder = hex.sortingOrder - 50;
        }
    }
}
