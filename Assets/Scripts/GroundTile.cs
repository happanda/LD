using UnityEngine;
using System.Collections;
using System;

public class GroundTile : MonoBehaviour
{
    private MovableHex moveHex;
    private HexSkin hexSkin;

    // event for Objectives class
    // newLvl == -1 on destroy
    public delegate void TileLevel(Tile Type, int oldLvl, int newLvl);
    static public event TileLevel tileLevel;
    

    public Tile Type;

    public int Level
    {
        get;
        private set;
    }

    void Awake()
    {
        Debug.Log("MovingHex.Awake");
        Level = 0;
        moveHex = GetComponent<MovableHex>();
        hexSkin = GetComponent<HexSkin>();
        LevelChanged(-1, 0);
    }

    void OnDestroy()
    {
        LevelChanged(Level, -1);
    }

    public void Upgrade()
    {
        if (Level < IslandManager.Inst.MaxLevel(Type))
        {
            ++Level;
            LevelChanged(Level - 1, Level);
        }
    }

    public void Downgrade()
    {
        if (Level > 0)
        {
            --Level;
            //Debug.Log("Level " + hexagon_.ToString() + ": " + level_);
            LevelChanged(Level + 1, Level);
        }
        else
        {
            IslandManager.Inst.Remove(moveHex.hexagon);
            IslandManager.Inst.CreateEmptyTile(moveHex.hexagon);
        }
    }

    private void LevelChanged(int oldLvl, int newLvl)
    {
        hexSkin.SetLevel(newLvl);
        if (tileLevel != null)
            tileLevel(Type, oldLvl, newLvl);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Fragment")
        {
            MovingFragment frag = other.GetComponent<MovingFragment>();
            if (frag.Type == Type)
                Upgrade();
            else
                Downgrade();
        }
        else if (other.tag == "Meteor")
        {
            MovingFragment frag = other.GetComponent<MovingFragment>();
            IslandManager.Inst.Remove(moveHex.hexagon);
        }
    }
}
