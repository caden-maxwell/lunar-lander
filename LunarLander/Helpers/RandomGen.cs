using System;
using Microsoft.Xna.Framework;

namespace LunarLander.Helpers;

public class RandomGen : Random
{
    public float NextRange(float low, float high)
    {
        return MathHelper.Lerp(low, high, (float)this.NextDouble());
    }

    public Vector2 NextUnitVector()
    {
        float angle = (float)(NextDouble() * 2 * Math.PI);
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }

    public double NextGaussian(double mean, double stdDev)
    {
        if (usePrevious)
        {
            usePrevious = false;
            return mean + y2 * stdDev;
        }

        usePrevious = true;

        double x1;
        double x2;
        double z;

        do
        {
            x1 = 2 * NextDouble() - 1;
            x2 = 2 * NextDouble() - 1;
            z = x1 * x1 + x2 * x2;
        } while (z >= 1);

        z = Math.Sqrt(-2 * Math.Log(z) / z);
        double y1 = x1 * z;
        y2 = x2 * z;

        return mean + y1 * stdDev;
    }
    private bool usePrevious = false;
    double y2;
}
