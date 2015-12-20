using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System;
using System.Reflection;


public enum Tile
{
    [Description("Tree")]
    Main = 0,

    [Description("Desert")]
    Desert = 1,

    [Description("Field")]
    Field = 2,

    [Description("Forest")]
    Forest = 3,

    [Description("Hills")]
    Hills = 4,

    [Description("Mountains")]
    Mountains = 5,
}


public static class TileExt
{
    public static readonly int TilesCount = Enum.GetValues(typeof(Tile)).Length;

    /// <summary>
    /// Returns a random type of Tile except Main tile
    /// </summary>
    public static Tile Random()
    {
        return (Tile)UnityEngine.Random.Range(1, TilesCount);
    }

    /// <summary>
    /// Returns readable name for the Tile type.
    /// </summary>
    public static String Str(this Tile type)
    {
        FieldInfo fi = type.GetType().GetField(type.ToString());

        if (null != fi)
        {
            object[] attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs != null && attrs.Length > 0)
                return ((DescriptionAttribute)attrs[0]).Description;
        }
        return null;
    }

    /// <summary>
    ///  Returns the Tile prefab for this type of tile.
    /// </summary>
    public static GameObject TilePrefab(this Tile type)
    {
        return IslandManager.Inst.GetTilePrefab(type);
    }

    /// <summary>
    ///  Returns the fragment prefab for this type of tile.
    /// </summary>
    public static GameObject FragPrefab(this Tile type)
    {
        return IslandManager.Inst.GetFragPrefab(type);
    }
}
