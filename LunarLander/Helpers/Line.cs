using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace LunarLander.Helpers;

public class Line
{
    public Vector2 Start { get; private set; }
    public Vector2 End { get; private set; }

    public Line(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }

    public float DistX()
    {
        return End.X - Start.X;
    }

    public (Line, Line) Split()
    {
        Vector2 midpoint = (Vector2.Subtract(End, Start) / 2) + Start;
        return (new Line(Start, midpoint), new Line(midpoint, End));
    }

    public void DisplaceY(float disp, bool start)
    {
        Vector2 dispVec = new(0, disp);
        if (start)
            Start += dispVec;
        else
            End += dispVec;
    }

    public override string ToString()
    {
        return Start.ToString() + ", " + End.ToString();
    }

    /// <summary>
    /// Checks whether two lines segments are intersecting.<br />
    /// <see href="https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/"/><br />
    /// Got rid of special cases, since they are probably very unlikely to happen in this game.
    /// </summary>
    /// <returns></returns>
    public static bool Intersecting(Line line1, Line line2)
    {
        static bool orientation(Line line, Vector2 point)
        {
            Vector2 v1 = point - line.End;
            Vector2 v2 = point - line.Start;
            return Vector3.Cross(new Vector3(v1, 0), new Vector3(v2, 0)).Z > 0;
        }

        bool o1 = orientation(line1, line2.Start);
        bool o2 = orientation(line1, line2.End);
        bool o3 = orientation(line2, line1.Start);
        bool o4 = orientation(line2, line1.End);

        if ((o1 != o2) && (o3 != o4))
            return true;

        return false;
    }
}
