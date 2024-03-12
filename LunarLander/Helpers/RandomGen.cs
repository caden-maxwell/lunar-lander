
using System;

namespace LunarLander.Helpers;

public class RandomGen
{
    private readonly Random m_random = new();

    public int Next()
    {
        return m_random.Next();
    }

    public double NextDouble()
    {
        return m_random.NextDouble();
    }

    public int NextRange(int low, int high)
    {
        int range = high - low + 1;
        return (int)Math.Floor(m_random.NextDouble() * range) + low;
    }

    public System.Numerics.Vector2 nextCircleVector()
    {
        float angle = (float)(m_random.NextDouble() * 2 * Math.PI);
        return new System.Numerics.Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
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
        double y1;
        double z;

        do
        {
            x1 = 2 * NextDouble() - 1;
            x2 = 2 * NextDouble() - 1;
            z = x1 * x1 + x2 * x2;
        } while (z >= 1);

        z = Math.Sqrt(-2 * Math.Log(z) / z);
        y1 = x1 * z;
        y2 = x2 * z;

        return mean + y1 * stdDev;
    }
    private bool usePrevious = false;
    double y2;
}
