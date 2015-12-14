using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


public static class ObjectiveExt
{
    public static KeyValuePair<Tile, int> IndexOf(this Objective obj)
    {
        Tile type = (Tile)((int)obj / 3);
        int lvl = (int)obj % 3 + 1;
        return new KeyValuePair<Tile, int>(type, lvl);
    }

    public static String Str(this Objective obj)
    {
        Tile type = (Tile)((int)obj / 3);
        int lvl = (int)obj % 3 + 1;
        return type.Str() + " of " + lvl + " level";
    }
}

public enum Objective
{
    None                = -1,
    UpgradeTower1       = 0,
    UpgradeTower2       = 1,
    UpgradeTower3       = 2,
    UpgradeDesert1      = 3,
    UpgradeField1       = 6,
    UpgradeField2       = 7,
    UpgradeField3       = 8,
    UpgradeForest1      = 9,
    UpgradeForest2      = 10,
    UpgradeForest3      = 11,
    UpgradeHills1       = 12,
    UpgradeHills2       = 13,
    UpgradeHills3       = 14,
    UpgradeMountains1   = 15,
    UpgradeMountains2   = 16,
}

[System.Serializable]
public struct ObjectiveCount
{
    public Objective obj;
    public int count;

    public override string ToString()
    {
        return "Create " + count + " " + obj.Str();
    }
}

public class Objectives : MonoBehaviour
{
    public ObjectiveCount[] objectives;

    private IDictionary<Tile, IDictionary<int, int>> levels = new Dictionary<Tile, IDictionary<int, int>>();

    void Awake()
    {
        levels[Tile.Desert] = new Dictionary<int, int>();
        levels[Tile.Field] = new Dictionary<int, int>();
        levels[Tile.Forest] = new Dictionary<int, int>();
        levels[Tile.Hills] = new Dictionary<int, int>();
        levels[Tile.Main] = new Dictionary<int, int>();
        levels[Tile.Mountains] = new Dictionary<int, int>();
        foreach (Tile t in Enum.GetValues(typeof(Tile)))
        {
            if (levels.ContainsKey(t))
            {
                for (int i = 0; i <= IslandManager.Inst.maxLevels[(int)t].maxLevel; ++i)
                    levels[t][i] = 0;
            }
        }
        MovingHex.tileLevel += OnTileLevel;

        foreach (var obj in objectives)
        {
            Debug.Log(obj.ToString());
        }
    }

    void OnDestroy()
    {
        MovingHex.tileLevel -= OnTileLevel;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTileLevel(Tile type, int oldLvl, int newLvl)
    {
        try
        {
            --levels[type][oldLvl];
            if (newLvl > 0)
                ++levels[type][newLvl];
        }
        catch (KeyNotFoundException ex)
        {
            Debug.LogError("Key not found: " + ex.Message);
        }
        if (CheckWinCondition())
            LevelFinished();
    }

    private void LevelFinished()
    {
        Debug.Log("LEVEL FINISHED");
    }

    private bool CheckWinCondition()
    {
        if (objectives == null)
            return false;
        foreach (ObjectiveCount oc in objectives)
        {
            if (oc.obj == Objective.None)
                continue;
            var p = oc.obj.IndexOf();
            if (levels[p.Key][p.Value] < oc.count)
                return false;
        }
        return true;
    }

    void OnGUI()
    {
        if (CheckWinCondition())
        {
            GUI.Label(new Rect(10, 10, 100, 20), "Level Is Finished!");
        }

        else
        {
            string objectiveText = "";
            foreach (ObjectiveCount oc in objectives)
            {
                if (oc.obj == Objective.None)
                    continue;
                var p = oc.obj.IndexOf();
                if (levels[p.Key][p.Value] < oc.count)
                    objectiveText += oc.ToString() + "\n";
            }
            GUI.Label(new Rect(10, 10, 1000, 200), objectiveText);
        }
    }
}
