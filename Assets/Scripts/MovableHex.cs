using UnityEngine;
using System.Collections;

/// <summary>
/// Support for moving in the hexagonal grid.
/// </summary>
public class MovableHex : MonoBehaviour
{
    private Hexagon hexagon_;       // logical coordinates
    private Vector3 destination_;   // screen coordinates of hexagon_
    private float distance_;        // distance to destination_

    public Hexagon hexagon
    {
        get { return this.hexagon_; }
        set
        {
            hexagon_ = value;
            CacheDestination();
        }
    }

    public void SetCoordinates(int q, int r)
    {
        hexagon = new Hexagon(q, r);
    }

    public float DistToTarget
    {
        get { return distance_; }
    }

    public void Update()
    {
        CacheDistToTarget();
        MoveToTarget();
    }

    public void Turn(bool left)
    {
        Hexagon newHex = hexagon.Rotate(left);
        hexagon = newHex;
    }

    protected bool MoveToTarget()
    {
        if (DistToTarget > float.Epsilon)
        {
            Vector3 newPos = Vector3.Slerp(transform.position, destination_, IslandManager.Inst.animationSpeed * Time.deltaTime);
            transform.position = newPos;
            return true;
        }
        return false;
    }

    private void CacheDestination()
    {
        destination_ = Layout.HexagonToPixel(IslandManager.Inst.layout, hexagon);
        CacheDistToTarget();
    }

    private void CacheDistToTarget()
    {
        distance_ = (transform.position - destination_).magnitude;
    }

}
