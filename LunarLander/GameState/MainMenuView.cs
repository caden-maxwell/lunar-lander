using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace LunarLander;

public class MainMenuView : GameStateView
{
    private SpriteFont m_fontMenu;
    private SpriteFont m_fontMenuHover;
    private Texture2D m_blankTex;
    private Texture2D m_backgroundTex;
    private Rectangle m_rect = new();

    public override GameStateEnum State { get; } = GameStateEnum.MainMenu;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.MainMenu;

    private enum MenuState
    {
        NewGame,
        HighScores,
        Settings,
        Credits,
        Quit,
        N
    }

    private MenuState m_currentSelection = MenuState.NewGame;

    public override void RegisterKeys()
    {
        m_inputDevice.RegisterCommand(Keys.Down, true, new CommandDelegate(SelectBelow));
        m_inputDevice.RegisterCommand(Keys.Up, true, new CommandDelegate(SelectAbove));
        m_inputDevice.RegisterCommand(Keys.Enter, true, new CommandDelegate(EnterPressed));
        m_inputDevice.RegisterCommand(Keys.Escape, true, new CommandDelegate(EscPressed));
    }

    public override void Reload() { }

    public override void LoadContent(ContentManager contentManager)
    {
        m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
        m_fontMenuHover = contentManager.Load<SpriteFont>("Fonts/menu-hover");

        m_blankTex = contentManager.Load<Texture2D>("Images/blank");
        m_backgroundTex = contentManager.Load<Texture2D>("Images/starry");
    }

    public override void Update(GameTime gameTime) { }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        m_rect.Location = new(0, 0);
        m_rect.Size = new(m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight);
        m_spriteBatch.Draw(
            m_backgroundTex,
            m_rect,
            null,
            Color.White,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );

        m_rect.Location = new((int)(m_graphics.PreferredBackBufferWidth * 0.5f) - 295, (int)(m_graphics.PreferredBackBufferHeight * 0.3f) - 95);
        m_rect.Size = new(600, 500);
        m_spriteBatch.Draw(
            m_blankTex,
            m_rect,
            null,
            Color.Black,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );
        m_rect.X -= 5;
        m_rect.Y -= 5;
        m_spriteBatch.Draw(
            m_blankTex,
            m_rect,
            null,
            Color.Gray,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );

        Dictionary<string, MenuState> keyValuePairs = new()
        {
            { "New Game", MenuState.NewGame },
            { "High Scores", MenuState.HighScores },
            { "Settings", MenuState.Settings },
            { "Credits", MenuState.Credits },
            { "Quit", MenuState.Quit },
        };

        float top = m_graphics.PreferredBackBufferHeight * 0.3f;
        foreach (KeyValuePair<string, MenuState> entry in keyValuePairs)
        {
            top = DrawMenuItem(
                m_currentSelection == entry.Value ? m_fontMenuHover : m_fontMenu,
                entry.Key,
                top,
                m_currentSelection == entry.Value ? Color.MediumBlue : Color.White
            );
        }

        m_spriteBatch.End();
    }

    private float DrawMenuItem(SpriteFont font, string text, float y, Color color)
    {
        Vector2 stringSize = font.MeasureString(text);
        float textX = m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2;
        m_spriteBatch.DrawString(
            font,
            text,
            new Vector2(textX + 3, y + 3),
            Color.Black
        );

        m_spriteBatch.DrawString(
            font,
            text,
            new Vector2(textX, y),
            color
        );

        return y + stringSize.Y;
    }

    private void SelectBelow(GameTime gameTime, float value)
    {
        int i = (int)m_currentSelection + 1;
        i %= (int)MenuState.N;
        m_currentSelection = (MenuState)i;
    }

    private void SelectAbove(GameTime gameTime, float value)
    {
        int i = (int)m_currentSelection - 1 + (int)MenuState.N; // C# mod doesn't handle negative values well, need to add N
        i %= (int)MenuState.N;
        m_currentSelection = (MenuState)i;
    }

    private void EnterPressed(GameTime gameTime, float value)
    {
        GameStateEnum state = m_currentSelection switch
        {
            MenuState.NewGame => GameStateEnum.GamePlay,
            MenuState.HighScores => GameStateEnum.HighScores,
            MenuState.Credits => GameStateEnum.Credits,
            MenuState.Settings => GameStateEnum.Settings,
            MenuState.Quit => GameStateEnum.Exit,
            _ => throw new NotImplementedException()
        };

        ChangeState(state);
    }

    public override void EscPressed(GameTime gameTime, float value)
    {
        ChangeState(GameStateEnum.Exit);
    }
}