using System;
using Microsoft.Xna.Framework;

namespace LunarLander.Helpers;

public class LunarMath
{
    /// <summary>
    /// Convert an angle in radians to a Vector2 direction.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector2 AngleToDirection(float angle)
    {
        return new Vector2(
          (float)Math.Cos(angle),
          (float)Math.Sin(angle)
        );
    }

    /// <summary>
    /// Convert a Vector2 direction to an angle in radians.
    /// </summary>
    /// <param name="Direction"></param>
    /// <returns></returns>
    public static float DirectionToAngle(Vector2 Direction)
    {
        return (float)Math.Atan2(Direction.Y, Direction.X);
    }
}
