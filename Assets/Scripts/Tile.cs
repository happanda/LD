using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System;
using System.Reflection;


public enum Tile
{
    [Description("Tree")]
    [PrefabNameAttribute("MainTilePrefab")]
    Main = 0,

    [Description("Desert")]
    [PrefabNameAttribute("DesertTilePrefab")]
    Desert = 1,

    [Description("Field")]
    [PrefabNameAttribute("FieldTilePrefab")]
    Field = 2,

    [Description("Forest")]
    [PrefabNameAttribute("ForestTilePrefab")]
    Forest = 3,

    [Description("Hills")]
    [PrefabNameAttribute("HillsTilePrefab")]
    Hills = 4,

    [Description("Mountains")]
    [PrefabNameAttribute("MountainsTilePrefab")]
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
    ///  Returns the prefab for this type of tile.
    /// </summary>
    public static GameObject Prefab(this Tile type)
    {
        FieldInfo fi = type.GetType().GetField(type.ToString());

        if (null != fi)
        {
            object[] attrs = fi.GetCustomAttributes(typeof(PrefabNameAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                string prefabName = ((PrefabNameAttribute)attrs[0]).Name;
                return GameObject.Find(prefabName);
            }
        }
        return null;
    }
}


