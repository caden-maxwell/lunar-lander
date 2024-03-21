using LunarLander.Input;
using LunarLander.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LunarLander;

public class HighScoresView : GameStateView
{
    private SpriteFont m_font;
    private SpriteFont m_fontBig;

    public override GameStateEnum State { get; } = GameStateEnum.HighScores;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.HighScores;
    private readonly Database m_storage;
    private int m_page = 1;

    public HighScoresView(Database storage)
    {
        m_storage = storage;
    }

    public override void Reload()
    {
        m_storage.LoadScores();
    }

    public override void RegisterKeys()
    {
        base.RegisterKeys();
        m_inputDevice.RegisterCommand(Keys.Right, true, new CommandDelegate(NextPage));
        m_inputDevice.RegisterCommand(Keys.Left, true, new CommandDelegate(PrevPage));
    }

    public override void LoadContent(ContentManager contentManager)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/menu");
        m_fontBig = contentManager.Load<SpriteFont>("Fonts/menu-hover");
    }
    public override void Update(GameTime gameTime) { }

    private void NextPage(GameTime gameTime, float value)
    {
        if (m_page + 1 > 5)
            return;
        m_page++;
    }

    private void PrevPage(GameTime gameTime, float value)
    {
        if (m_page - 1 < 1)
            return;
        m_page--;
    }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        string text = "HIGH SCORES";
        Vector2 stringSize = m_fontBig.MeasureString(text);
        float textX = m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2;
        float textY = m_graphics.PreferredBackBufferHeight * 0.05f;
        m_spriteBatch.DrawString(
            m_fontBig,
            text,
            new Vector2(textX, textY),
            Color.Yellow
        );
        textY += stringSize.Y;

        text = $"LEVEL {m_page}";
        stringSize = m_font.MeasureString(text);
        textX = m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2;
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.Yellow
        );
        textY += stringSize.Y + 50;

        GameScore score;
        for (int i = 0; i < 5; i++)
        {
            text = "-----------";
            if (m_storage.Scores[m_page].Count > i)
            {
                score = m_storage.Scores[m_page][i];
                text = $"{score.Name} | {score.Score:0.00}";
            }
            
            stringSize = m_font.MeasureString(text);
            textX = m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2;
            m_spriteBatch.DrawString(
                m_font,
                text,
                new Vector2(textX, textY),
                Color.Yellow
            );
            textY += stringSize.Y;
        }
        textY += 50;

        text = "";
        if (m_page != 1)
            text += "<-- ";
        stringSize = m_font.MeasureString(text);
        if (m_page != 5)
            text += " -->";
        stringSize = m_font.MeasureString(text);
        textX = m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2;
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.Yellow
        );

        m_spriteBatch.End();
    }
}
