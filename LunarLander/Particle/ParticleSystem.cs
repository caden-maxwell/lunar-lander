using LunarLander.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LunarLander.Particle;

public class ParticleSystem
{
    private readonly Dictionary<long, Particle> m_particles = new();
    public Dictionary<long, Particle>.ValueCollection Particles { get { return m_particles.Values; } }
    private readonly RandomGen m_random = new();

    private readonly float m_particlesPerMS;
    private float m_remainderParticles = 0;

    public ParticleSystem(float thrustPPMS)
    {
        m_particlesPerMS = thrustPPMS; // Particles per millisecond
    }

    public void ShipThrust(float elapsed, Vector2 center, Vector2 direction, float speed, float size, float lifetime)
    {
        m_remainderParticles += elapsed * m_particlesPerMS;
        int numParticles = (int)m_remainderParticles;
        m_remainderParticles -= numParticles;

        float angle = LunarMath.DirectionToAngle(direction);
        float gaussianSize;
        float gaussianAngle;
        for (int i = 0; i < numParticles; ++i)
        {
            gaussianAngle = (float)m_random.NextGaussian(angle, MathHelper.PiOver4 / 4);
            gaussianSize = (float)(size * m_random.NextGaussian(1, 0.5f));
            Particle particle = new(
                center,
                LunarMath.AngleToDirection(gaussianAngle),
                (float)(speed * m_random.NextGaussian(1, 0.3f)),
                new Vector2(gaussianSize, gaussianSize),
                new System.TimeSpan(0, 0, 0, 0, (int)(lifetime * m_random.NextGaussian(1, 0.5f)))
            );
            m_particles.Add(particle.name, particle);
        }
    }

    public void ShipCrash(int particleAmt, Vector2 center, float speed, float size, float lifetime)
    {
        float gaussianSize;
        for (int i = 0; i < particleAmt; i++)
        {
            gaussianSize = (float)(size * m_random.NextGaussian(0, 1));
            Particle particle = new(
                center,
                m_random.NextUnitVector(),
                (float)(speed * m_random.NextGaussian(2, 1)),
                new Vector2(gaussianSize, gaussianSize),
                new System.TimeSpan(0, 0, 0, 0, (int)(lifetime * m_random.NextGaussian(1, 0.2f)))
            );
            m_particles.Add(particle.name, particle);
        }
    }

    public void Update(GameTime gameTime)
    {
        // Update existing particles
        List<long> toRemove = new();
        foreach (Particle p in m_particles.Values)
            if (!p.Update(gameTime))
                toRemove.Add(p.name);

        // Remove dead particles
        foreach (long key in toRemove)
            m_particles.Remove(key);
    }

    public void Clear()
    {
        m_particles.Clear();
    }
}