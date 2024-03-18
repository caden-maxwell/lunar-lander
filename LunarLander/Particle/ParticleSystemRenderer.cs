using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LunarLander.Particle;

public class ParticleSystemRenderer
{
    private readonly string m_nameParticleContent;
    private Texture2D m_texParticle;

    public ParticleSystemRenderer(string nameParticleContent)
    {
        m_nameParticleContent = nameParticleContent;
    }

    public void LoadContent(ContentManager content)
    {
        m_texParticle = content.Load<Texture2D>(m_nameParticleContent);
    }

    public void Render(SpriteBatch spriteBatch, ParticleSystem system)
    {
        Rectangle rect = new(0, 0, 0, 0);
        Vector2 centerTexture = new(m_texParticle.Width / 2, m_texParticle.Height / 2);
        foreach (Particle particle in system.Particles)
        {
            rect.X = (int)particle.center.X;
            rect.Y = (int)particle.center.Y;
            rect.Width = (int)particle.size.X;
            rect.Height = (int)particle.size.Y;

            spriteBatch.Draw(
                m_texParticle,
                rect,
                null,
                Color.White,
                particle.rotation,
                centerTexture,
                SpriteEffects.None,
                0
            );
        }
    }
}
