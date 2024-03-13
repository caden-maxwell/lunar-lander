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
    /// Checks whether two lines segments are intersecting. See: 
    /// https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns></returns>
    public static bool Intersecting(Line line1, Line line2)
    {
        static int orientation(Line line, Vector2 p)
        {
            Vector2 v1 = p - line.End;
            Vector2 v2 = p - line.Start;
            float res = Vector3.Cross(new Vector3(v1, 0), new Vector3(v2, 0)).Z;
            if (res > 0)
                return 1; // Clockwise
            else if (res < 0)
                return 2; // Counterclockwise
            return 0; // Colinear
        }

        int o1 = orientation(line1, line2.Start);
        int o2 = orientation(line1, line2.End);
        int o3 = orientation(line2, line1.Start);
        int o4 = orientation(line2, line1.End);

        if ((o1 != o2) && (o3 != o4))
            return true;

        // Got rid of special cases, since they were very unlikely to happen in this game.
        return false;
    }
}
