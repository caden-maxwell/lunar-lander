﻿using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LunarLander;

public class MainMenuView : GameStateView
{
    private SpriteFont m_fontMenu;
    private SpriteFont m_fontMenuSelect;

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

    public override void RegisterKeys(IInputDevice inputDevice)
    {
        inputDevice.RegisterCommand(Keys.Down, true, new CommandDelegate(SelectBelow));
        inputDevice.RegisterCommand(Keys.Up, true, new CommandDelegate(SelectAbove));
        inputDevice.RegisterCommand(Keys.Enter, true, new CommandDelegate(EnterPressed));
        inputDevice.RegisterCommand(Keys.Escape, true, new CommandDelegate(EscPressed));
    }

    public override void Reload() { }

    public override void LoadContent(ContentManager contentManager)
    {
        m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
        m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
    }

    public override void Update(GameTime gameTime) { }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        // I split the first one's parameters on separate lines to help you see them better
        float bottom = DrawMenuItem(
            m_currentSelection == MenuState.NewGame ? m_fontMenuSelect : m_fontMenu,
            "New Game",
            200,
            m_currentSelection == MenuState.NewGame ? Color.Yellow : Color.Blue);
        bottom = DrawMenuItem(m_currentSelection == MenuState.HighScores ? m_fontMenuSelect : m_fontMenu, "High Scores", bottom, m_currentSelection == MenuState.HighScores ? Color.Yellow : Color.Blue);
        bottom = DrawMenuItem(m_currentSelection == MenuState.Settings ? m_fontMenuSelect : m_fontMenu, "Settings", bottom, m_currentSelection == MenuState.Settings ? Color.Yellow : Color.Blue);
        bottom = DrawMenuItem(m_currentSelection == MenuState.Credits ? m_fontMenuSelect : m_fontMenu, "Credits", bottom, m_currentSelection == MenuState.Credits ? Color.Yellow : Color.Blue);
        DrawMenuItem(m_currentSelection == MenuState.Quit ? m_fontMenuSelect : m_fontMenu, "Quit", bottom, m_currentSelection == MenuState.Quit ? Color.Yellow : Color.Blue);

        m_spriteBatch.End();
    }

    private float DrawMenuItem(SpriteFont font, string text, float y, Color color)
    {
        Vector2 stringSize = font.MeasureString(text);
        m_spriteBatch.DrawString(
            font,
            text,
            new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y),
            color);

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
        switch (m_currentSelection)
        {
            case MenuState.NewGame:
                ChangeState(GameStateEnum.GamePlay);
                break;
            case MenuState.HighScores:
                ChangeState(GameStateEnum.HighScores);
                break;
            case MenuState.Credits:
                ChangeState(GameStateEnum.Credits);
                break;
            case MenuState.Settings:
                ChangeState(GameStateEnum.Settings);
                break;
            case MenuState.Quit:
                ChangeState(GameStateEnum.Exit);
                break;
        }
    }

    public override void EscPressed(GameTime gameTime, float value)
    {
        ChangeState(GameStateEnum.Exit);
    }
}