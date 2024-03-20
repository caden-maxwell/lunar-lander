using LunarLander.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LunarLander;

public class HighScoresView : GameStateView
{
    private SpriteFont m_font;

    public override GameStateEnum State { get; } = GameStateEnum.HighScores;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.HighScores;
    private readonly Database m_storage;

    public HighScoresView(Database storage)
    {
        m_storage = storage;
    }

    public override void Reload() { }

    public override void LoadContent(ContentManager contentManager)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/menu");
    }
    public override void Update(GameTime gameTime) { }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        string text = "";
        foreach (GameScore score in m_storage.Scores)
        {
            text += $"{score.Name} | {score.Level} | {score.Score}\n";
        }

        Vector2 stringSize = m_font.MeasureString(text);
        m_spriteBatch.DrawString(m_font, text,
            new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, m_graphics.PreferredBackBufferHeight / 2 - stringSize.Y), Color.Yellow);

        m_spriteBatch.End();
    }
}
