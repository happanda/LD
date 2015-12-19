using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public struct TileSpritePair
{
    public Tile type;
    public Sprite[] sprites;
}

public class AssetManager : MonoBehaviour
{
        /// AssetManager singleton
    static public AssetManager Inst;

        /// Array filled from the inspector, copied into the 'sprites' dictionary in Awake()
    public TileSpritePair[] tileSprites;


    private IDictionary<Tile, Sprite[]> sprites;


    void Awake()
    {
        Debug.Log("AssetManager.Awake");
        if (Inst == null)
            Inst = this;
        else if (Inst != this)
        {
            Destroy(gameObject);
            return;
        }

        foreach (var it in tileSprites)
        {
            sprites[it.type] = it.sprites;
        }
    }

}
