using System;
using Microsoft.Xna.Framework;

namespace LunarLander.Helpers;

public class RandomGen : Random
{
    public float NextRange(float low, float high)
    {
        return MathHelper.Lerp(low, high, (float)NextDouble());
    }

    public Vector2 NextUnitVector()
    {
        float angle = (float)(NextDouble() * 2 * Math.PI);
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }

    private double spare;
    private bool hasSpare = false;
    /// <summary> See:
    /// <see href="https://en.wikipedia.org/wiki/Marsaglia_polar_method"/>
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="stdDev"></param>
    /// <returns></returns>
    public double NextGaussian(double mean, double stdDev)
    {
        if (hasSpare)
        {
            hasSpare = false;
            return mean + spare * stdDev;
        }

        double u, v, s;
        do
        {
            u = 2.0 * NextDouble() - 1.0;
            v = 2.0 * NextDouble() - 1.0;
            s = u * u + v * v;
        } while (s >= 1.0 || s == 0);

        s = Math.Sqrt(-2.0 * Math.Log(s) / s);
        spare = v * s;
        hasSpare = true;

        return mean + stdDev * u * s;
    }
}
