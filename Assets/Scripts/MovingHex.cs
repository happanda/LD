using UnityEngine;
using System.Collections;

public class MovingHex : MonoBehaviour
{
    private Hexagon hexagon_; // actual coordinates

    public float animationSpeed = 7f;
    public Tile type; // type of the tile
    private int level; // level if upgrade

    // event for HexSkin of this same hex
    public delegate void LevelChanged(int level);
    public event LevelChanged levelChanged;

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
        }
    }

    public void SetCoordinates(int q, int r)
    {
        hexagon = new Hexagon(q, r);
    }

    void Awake()
    {
        Debug.Log("MovingHex.Awake");
        level = 0;
    }

    void OnDestroy()
    {
        tileLevel(type, level, -1);
    }

    // Update is called once per frame
    void Update()
    {
        Point pnt = Layout.HexagonToPixel(IslandManager.Inst.layout, hexagon);
        Vector3 dest = new Vector3(pnt.x, pnt.y, 0f);
        float dist = (transform.position - dest).sqrMagnitude;
        if (dist > float.Epsilon)
        {
            //Vector3 newPos = Vector3.RotateTowards(transform.position, dest, 10f * Time.deltaTime, 0f);
            //Vector3 newPos = Vector3.MoveTowards(transform.position, dest, 3f * Time.deltaTime);
            Vector3 newPos = Vector3.Slerp(transform.position, dest, animationSpeed * Time.deltaTime);
            transform.position = newPos;
        }
    }

    public bool InBarrier()
    {
        return IslandManager.Inst.InBarrier(hexagon_);
    }

    public void Upgrade()
    {
        if (level < IslandManager.Inst.MaxLevel(type))
        {
            ++level;
            Debug.Log("Level " + hexagon_.ToString() + ": " + level);
            if (levelChanged != null)
                levelChanged(level);
            if (tileLevel != null)
                tileLevel(type, level - 1, level);
        }
    }

    public void Downgrade()
    {
        if (level > 0)
        {
            --level;
            Debug.Log("Level " + hexagon_.ToString() + ": " + level);
            if (levelChanged != null)
                levelChanged(level);
            if (tileLevel != null)
                tileLevel(type, level + 1, level);
        }
        else
            IslandManager.Inst.Remove(hexagon_);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Fragment")
        {
            MovingFragment frag = other.GetComponent<MovingFragment>();
            if (InBarrier())
            {
                // decide which side was hit
                Vector3 dir = other.transform.position - transform.position;
                float angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
                if (angle < 0)
                    angle += 360f;
                Hexagon newHex = Hexagon.Neighbor(hexagon_, Mathf.RoundToInt(angle) / 60);
                IslandManager.Inst.Attach(newHex, frag.type);
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
