using UnityEngine;
using System.Collections;

public class EmptyHex : MonoBehaviour
{
    private MovableHex moveHex;

    void Awake()
    {
        moveHex = GetComponent<MovableHex>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        return;
        if (other.tag == "Fragment")
        {
            Hexagon hex = moveHex.hexagon;
            IslandManager.Inst.Remove(hex);
            MovingFragment frag = other.GetComponent<MovingFragment>();
            IslandManager.Inst.CreateTile(hex, frag.Type);
        }
    }

    void OnDestroy()
    {
    }
}
