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
        if (other.tag == "Fragment")
        {
            MovingFragment frag = other.GetComponent<MovingFragment>();
            if (frag.Vertical == moveHex.hexagon)
            {
                Hexagon hex = moveHex.hexagon;
                IslandManager.Inst.Remove(hex);
                IslandManager.Inst.CreateTile(hex, frag.Type);
            }
        }
    }

    void OnDestroy()
    {
    }
}
