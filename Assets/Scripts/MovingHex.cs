using UnityEngine;
using System.Collections;

public class MovingHex : MonoBehaviour
{
    private Hexagon hexagon_; // actual coordinates

    public Tile type; // type of the tile
    private int level; // level if upgrade

    private HexSkin hexSkin;

    // event for Objectives class
    // newLvl == -1 on destroy
    public delegate void TileLevel(Tile type, int oldLvl, int newLvl);
    static public event TileLevel tileLevel;

    public Hexagon hexagon
    {
        get { return this.hexagon_; }
        set
        {
            hexagon_ = value;
            hexSkin.UpdateSortingOrder(hexagon);
        }
    }

    public void SetCoordinates(int q, int r)
    {
        hexagon = new Hexagon(q, r);
    }

    void Awake()
    {
        Debug.Log("MovingHex.Awake");
        level = -1;
        hexSkin = GetComponent<HexSkin>();
        hexSkin.SetLevel(level);
    }

    void OnDestroy()
    {
        if (tileLevel != null)
            tileLevel(type, level, -1);
    }

    void Update()
    {
        Vector3 dest = Layout.HexagonToPixel(IslandManager.Inst.layout, hexagon);
        float dist = (transform.position - dest).sqrMagnitude;
        if (dist > float.Epsilon)
        {
            Vector3 newPos = Vector3.Slerp(transform.position, dest, IslandManager.Inst.animationSpeed * Time.deltaTime);
            transform.position = newPos;
            hexSkin.UpdateSortingOrder(hexagon);
        }
    }

    public void Upgrade()
    {
        if (level < IslandManager.Inst.MaxLevel(type))
        {
            ++level;
            //Debug.Log("Level " + hexagon_.ToString() + ": " + level);
            hexSkin.SetLevel(level);
            if (tileLevel != null)
                tileLevel(type, level - 1, level);
        }
    }

    public void Downgrade()
    {
        if (level > 0)
        {
            --level;
            //Debug.Log("Level " + hexagon_.ToString() + ": " + level);
            hexSkin.SetLevel(level);
            if (tileLevel != null)
                tileLevel(type, level + 1, level);
        }
        else
            IslandManager.Inst.Remove(hexagon_);
    }

    private bool InBarrier()
    {
        return IslandManager.Inst.InBarrier(hexagon_);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Fragment")
        {
            MovingFragment frag = other.GetComponent<MovingFragment>();
            if (level == -1)
            {
                type = frag.type;
                Upgrade();
            }
            else
            {
                if (frag.type == type)
                    Upgrade();
                else
                    Downgrade();
            }
        }
        else if (other.tag == "Meteor")
        {
            MovingFragment frag = other.GetComponent<MovingFragment>();
            IslandManager.Inst.Remove(hexagon_);
        }
    }
}
