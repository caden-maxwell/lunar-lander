using System;
using Microsoft.Xna.Framework;

namespace LunarLander.Helpers;

public class LunarMath
{
    public static Vector2 AngleToDirection(float angle)
    {
        return new Vector2(
          (float)Math.Cos(angle),
          (float)Math.Sin(angle)
        );
    }

    public static float DirectionToAngle(Vector2 Direction)
    {
        return (float)Math.Atan2(Direction.Y, Direction.X);
    }
}
