using Microsoft.Xna.Framework;

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
}
