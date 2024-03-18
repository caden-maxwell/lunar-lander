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
    }

    public override void Update(GameTime gameTime) { }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        Dictionary<string, MenuState> keyValuePairs = new Dictionary<string, MenuState>()
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
        m_spriteBatch.DrawString(
            font,
            text,
            new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2 + 2, y + 2),
            Color.Black
        );

        m_spriteBatch.DrawString(
            font,
            text,
            new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y),
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