using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Hex : MonoBehaviour
{
    public const float Size = 0.85f;
    public const float Yscale = 0.53f;
}

public class Point
{
    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public readonly float x;
    public readonly float y;
}

public enum Dir
{
    RU,
    U,
    LU,
    LD,
    D,
    RD,
}

public class Hexagon : System.IEquatable<Hexagon>
{
    public Hexagon(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }
    public Hexagon(int q, int r)
    {
        this.q = q;
        this.r = r;
        this.s = -q - r;
    }
    public readonly int q;
    public readonly int r;
    public readonly int s;

    public Hexagon Rotate(bool left)
    {
        int radius = Hexagon.Length(this);
        IList<Hexagon> ring = Ring(new Hexagon(0, 0), radius);

        int idx = ring.IndexOf(this);
        Debug.Assert(idx >= 0, "Tile is not present in it's own ring");
        return ring[(idx + (left ? radius : ring.Count - radius)) % ring.Count];
    }

    static public Hexagon Add(Hexagon a, Hexagon b)
    {
        return new Hexagon(a.q + b.q, a.r + b.r, a.s + b.s);
    }


    static public Hexagon Subtract(Hexagon a, Hexagon b)
    {
        return new Hexagon(a.q - b.q, a.r - b.r, a.s - b.s);
    }


    static public Hexagon Scale(Hexagon a, int k)
    {
        return new Hexagon(a.q * k, a.r * k, a.s * k);
    }

    static public List<Hexagon> directions = new List<Hexagon>
        {
            new Hexagon(1, 0, -1), // RU
            new Hexagon(0, 1, -1), // U
            new Hexagon(-1, 1, 0), // LU
            new Hexagon(-1, 0, 1), // LD
            new Hexagon(0, -1, 1), // D
            new Hexagon(1, -1, 0), // RD
        };

    static public Hexagon Direction(int direction)
    {
        return Hexagon.directions[direction];
    }

    static public IList<Hexagon> Ring(Hexagon center, int radius)
    {
        IList<Hexagon> ring = new List<Hexagon>();
        if (radius == 0)
            ring.Add(center);
        else
        {
            Hexagon iter = Hexagon.Add(center, Hexagon.Scale(Hexagon.directions[0], radius));
            bool notFound = true;
            int shiftI = 2;

            for (int i = 0; i < 6 && notFound; ++i)
            {
                for (int j = 0; j < radius && notFound; ++j)
                {
                    iter = Hexagon.Neighbor(iter, (i + shiftI) % 6);
                    ring.Add(iter);
                }
            }
        }
        return ring;
    }

    static public Hexagon Neighbor(Hexagon Hexagon, int direction)
    {
        return Hexagon.Add(Hexagon, Hexagon.Direction(direction));
    }

    static public List<Hexagon> diagonals = new List<Hexagon>{new Hexagon(2, -1, -1), new Hexagon(1, -2, 1), new Hexagon(-1, -1, 2), new Hexagon(-2, 1, 1), new Hexagon(-1, 2, -1), new Hexagon(1, 1, -2)};

    static public Hexagon DiagonalNeighbor(Hexagon Hexagon, int direction)
    {
        return Hexagon.Add(Hexagon, Hexagon.diagonals[direction]);
    }


    static public int Length(Hexagon Hexagon)
    {
        return (int)((Mathf.Abs(Hexagon.q) + Mathf.Abs(Hexagon.r) + Mathf.Abs(Hexagon.s)) / 2);
    }


    static public int Distance(Hexagon a, Hexagon b)
    {
        return Hexagon.Length(Hexagon.Subtract(a, b));
    }

    public override int GetHashCode()
    {
        int hq = q.GetHashCode();
        int hr = r.GetHashCode();
        return (int)(hq ^ (hr + 0x9e3779b9 + (hq << 6) + (hq >> 2)));
    }

    public override bool Equals(System.Object other)
    {
        return (other is Hexagon) && (this == (Hexagon)other);
    }

    bool System.IEquatable<Hexagon>.Equals(Hexagon other)
    {
        return q == other.q && r == other.r && s == other.s;
    }

    public static bool operator ==(Hexagon a, Hexagon b)
    {
        return (a.q == b.q) && (a.r == b.r) && (a.s == b.s);
    }
    
    public static bool operator !=(Hexagon a, Hexagon b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return "(" + q + ", " + r + ", " + s + ")";
    }
}

public class FractionalHexagon
{
    public FractionalHexagon(float q, float r, float s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }
    public readonly float q;
    public readonly float r;
    public readonly float s;

    static public Hexagon HexagonRound(FractionalHexagon h)
    {
        int q = (int)(Mathf.Round(h.q));
        int r = (int)(Mathf.Round(h.r));
        int s = (int)(Mathf.Round(h.s));
        float q_diff = Mathf.Abs(q - h.q);
        float r_diff = Mathf.Abs(r - h.r);
        float s_diff = Mathf.Abs(s - h.s);
        if (q_diff > r_diff && q_diff > s_diff)
        {
            q = -r - s;
        }
        else
            if (r_diff > s_diff)
            {
                r = -q - s;
            }
            else
            {
                s = -q - r;
            }
        return new Hexagon(q, r, s);
    }


    static public FractionalHexagon HexagonLerp(Hexagon a, Hexagon b, float t)
    {
        return new FractionalHexagon(a.q + (b.q - a.q) * t, a.r + (b.r - a.r) * t, a.s + (b.s - a.s) * t);
    }


    static public List<Hexagon> HexagonLinedraw(Hexagon a, Hexagon b)
    {
        int N = Hexagon.Distance(a, b);
        List<Hexagon> results = new List<Hexagon>{};
        float step = 1.0f / Mathf.Max(N, 1);
        for (int i = 0; i <= N; i++)
        {
            results.Add(FractionalHexagon.HexagonRound(FractionalHexagon.HexagonLerp(a, b, step * i)));
        }
        return results;
    }

}

public class OffsetCoord
{
    public OffsetCoord(int col, int row)
    {
        this.col = col;
        this.row = row;
    }
    public readonly int col;
    public readonly int row;
    static public int EVEN = 1;
    static public int ODD = -1;

    static public OffsetCoord QoffsetFromCube(int offset, Hexagon h)
    {
        int col = h.q;
        int row = h.r + (int)((h.q + offset * (h.q & 1)) / 2);
        return new OffsetCoord(col, row);
    }


    static public Hexagon QoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col;
        int r = h.row - (int)((h.col + offset * (h.col & 1)) / 2);
        int s = -q - r;
        return new Hexagon(q, r, s);
    }


    static public OffsetCoord RoffsetFromCube(int offset, Hexagon h)
    {
        int col = h.q + (int)((h.r + offset * (h.r & 1)) / 2);
        int row = h.r;
        return new OffsetCoord(col, row);
    }


    static public Hexagon RoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col - (int)((h.row + offset * (h.row & 1)) / 2);
        int r = h.row;
        int s = -q - r;
        return new Hexagon(q, r, s);
    }

}

public class Orientation
{
    public Orientation(float f0, float f1, float f2, float f3, float b0, float b1, float b2, float b3, float start_angle)
    {
        this.f0 = f0;
        this.f1 = f1;
        this.f2 = f2;
        this.f3 = f3;
        this.b0 = b0;
        this.b1 = b1;
        this.b2 = b2;
        this.b3 = b3;
        this.start_angle = start_angle;
    }
    public readonly float f0;
    public readonly float f1;
    public readonly float f2;
    public readonly float f3;
    public readonly float b0;
    public readonly float b1;
    public readonly float b2;
    public readonly float b3;
    public readonly float start_angle;
}

public class Layout
{
    public Layout(Orientation orientation, Point size, Point origin)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }
    public readonly Orientation orientation;
    public readonly Point size;
    public readonly Point origin;
    static public Orientation pointy = new Orientation(Mathf.Sqrt(3.0f), Mathf.Sqrt(3.0f) / 2.0f, 0.0f, 3.0f / 2.0f, Mathf.Sqrt(3.0f) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f, 0.5f);
    static public Orientation flat = new Orientation(3.0f / 2.0f, 0.0f, Mathf.Sqrt(3.0f) / 2.0f, Mathf.Sqrt(3.0f), 2.0f / 3.0f, 0.0f, -1.0f / 3.0f, Mathf.Sqrt(3.0f) / 3.0f, 0.0f);

    static public Point HexagonToPixel(Layout layout, Hexagon h)
    {
        Orientation M = layout.orientation;
        Point size = layout.size;
        Point origin = layout.origin;
        float x = (M.f0 * h.q + M.f1 * h.r) * size.x;
        float y = (M.f2 * h.q + M.f3 * h.r) * size.y;
        return new Point(x + origin.x, y + origin.y);
    }


    static public FractionalHexagon PixelToHexagon(Layout layout, Point p)
    {
        Orientation M = layout.orientation;
        Point size = layout.size;
        Point origin = layout.origin;
        Point pt = new Point((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
        float q = M.b0 * pt.x + M.b1 * pt.y;
        float r = M.b2 * pt.x + M.b3 * pt.y;
        return new FractionalHexagon(q, r, -q - r);
    }


    static public Point HexagonCornerOffset(Layout layout, int corner)
    {
        Orientation M = layout.orientation;
        Point size = layout.size;
        float angle = 2.0f * Mathf.PI * (corner + M.start_angle) / 6f;
        return new Point(size.x * Mathf.Cos(angle), size.y * Mathf.Sin(angle));
    }


    static public List<Point> PolygonCorners(Layout layout, Hexagon h)
    {
        List<Point> corners = new List<Point>{};
        Point center = Layout.HexagonToPixel(layout, h);
        for (int i = 0; i < 6; i++)
        {
            Point offset = Layout.HexagonCornerOffset(layout, i);
            corners.Add(new Point(center.x + offset.x, center.y + offset.y));
        }
        return corners;
    }

}