using LunarLander.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace LunarLander.Particle;

public class ParticleSystem
{
    private Dictionary<long, Particle> m_particles = new();
    public Dictionary<long, Particle>.ValueCollection Particles { get { return m_particles.Values; } }
    private readonly RandomGen m_random = new();

    private Vector2 m_center;
    private readonly int m_sizeMean; // pixels
    private readonly int m_sizeStdDev;   // pixels
    private readonly float m_speedMean;  // pixels per millisecond
    private readonly float m_speedStDev; // pixles per millisecond
    private readonly float m_lifetimeMean; // milliseconds
    private readonly float m_lifetimeStdDev; // milliseconds
    private readonly float m_particlesPerMS;

    public ParticleSystem(Vector2 center, int sizeMean, int sizeStdDev, float speedMean, float speedStdDev, int lifetimeMean, int lifetimeStdDev, float particlePerMS)
    {
        m_center = center;
        m_sizeMean = sizeMean;
        m_sizeStdDev = sizeStdDev;
        m_speedMean = speedMean;
        m_speedStDev = speedStdDev;
        m_lifetimeMean = lifetimeMean;
        m_lifetimeStdDev = lifetimeStdDev;
        m_particlesPerMS = particlePerMS;
    }

    public void ShipThrust(float elapsed, Vector2 direction)
    {
        int numParticles = (int)(elapsed * m_particlesPerMS);
        float angle = LunarMath.DirectionToAngle(direction);
        angle = (float)m_random.NextGaussian(angle, MathHelper.Pi / 16);
        float size = (float)m_random.NextGaussian(m_sizeMean, m_sizeStdDev);
        Particle particle = new(
            m_center,
            LunarMath.AngleToDirection(angle),
            (float)Math.Abs(m_random.NextGaussian(m_speedMean, m_speedStDev)),
            new Vector2(size, size),
            new System.TimeSpan(0, 0, 0, 0, (int)(m_random.NextGaussian(m_lifetimeMean, m_lifetimeStdDev)))
        );

        m_particles.Add(particle.name, particle);
    }

    public void Update(GameTime gameTime)
    {
        // Update existing particles
        List<long> removeMe = new List<long>();
        foreach (Particle p in m_particles.Values)
            if (!p.Update(gameTime))
                removeMe.Add(p.name);

        // Remove dead particles
        foreach (long key in removeMe)
            m_particles.Remove(key);
    }

    public void SetCenter(Vector2 center)
    {
        m_center = center;
    }
}