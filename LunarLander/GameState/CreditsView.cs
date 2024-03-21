using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LunarLander;

public class CreditsView : GameStateView
{
    private SpriteFont m_font;
    private const string MESSAGE = "*I* (Caden Maxwell) wrote this amazing game!";

    public override GameStateEnum State { get; } = GameStateEnum.Credits;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.Credits;

    public override void Reload() { }

    public override void LoadContent(ContentManager contentManager)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/menu");
    }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        Vector2 stringSize = m_font.MeasureString(MESSAGE);
        float textX = m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2;
        float textY = m_graphics.PreferredBackBufferHeight / 2 - stringSize.Y;
        m_spriteBatch.DrawString(
            m_font,
            MESSAGE,
            new Vector2(textX + 3, textY + 3),
            Color.Black
        );
        m_spriteBatch.DrawString(
            m_font,
            MESSAGE,
            new Vector2(textX, textY),
            Color.White
        );

        m_spriteBatch.End();
    }

    public override void Update(GameTime gameTime) { }
}
