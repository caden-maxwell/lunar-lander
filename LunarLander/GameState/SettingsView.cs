using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace LunarLander;

public class SettingsView : GameStateView
{
    private SpriteFont m_fontMenu;
    private SpriteFont m_fontMenuHover;
    private readonly InputMapper m_inputMapper;

    public SettingsView(InputMapper inputMapper)
    {
        m_inputMapper = inputMapper;
    }

    public override GameStateEnum State { get; } = GameStateEnum.Settings;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.Settings;
    private enum MenuState
    {
        Thrust,
        RotateClockwise,
        RotateCounterClockwise,
        Back,
        N // Represents the length of this enum
    }
    private MenuState m_currentSelection = MenuState.Back;
    private bool m_signalAwaitingInput;
    private bool m_awaitingInput = false;
    private bool m_signalAwaitingEnded;

    public override void RegisterKeys()
    {
        base.RegisterKeys();
        m_inputDevice.RegisterCommand(Keys.Down, true, new CommandDelegate(SelectBelow));
        m_inputDevice.RegisterCommand(Keys.Up, true, new CommandDelegate(SelectAbove));
        m_inputDevice.RegisterCommand(Keys.Enter, true, new CommandDelegate(EnterPressed));
    }

    public override void LoadContent(ContentManager contentManager)
    {
        m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
        m_fontMenuHover = contentManager.Load<SpriteFont>("Fonts/menu-hover");
    }

    public override void Reload() { }

    public override void Update(GameTime gameTime)
    {
        if (m_signalAwaitingEnded)
        {
            m_signalAwaitingEnded = false;
            m_awaitingInput = false;
            m_inputDevice.UnregisterAll();
            RegisterKeys();
        }

        if (m_signalAwaitingInput)
        {
            m_signalAwaitingInput = false;
            m_awaitingInput = true;
            m_inputDevice.UnregisterAll();
            m_inputDevice.GetNextInput(new InputCallbackDelegate(NextInput));
            m_inputDevice.RegisterCommand(Keys.Escape, true, new CommandDelegate(Cancel));
        }
    }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        Dictionary<string, MenuState> keyValuePairs = new()
        {
            { $"Thrust: {m_inputMapper.KeyboardMappings[ActionEnum.Thrust]}", MenuState.Thrust },
            { $"Rotate Clockwise: {m_inputMapper.KeyboardMappings[ActionEnum.RotateClockwise]}", MenuState.RotateClockwise},
            { $"Rotate Counterclockwise: {m_inputMapper.KeyboardMappings[ActionEnum.RotateCounterClockwise]}", MenuState.RotateCounterClockwise },
            { "Back to Main Menu", MenuState.Back},
        };

        float top = m_graphics.PreferredBackBufferHeight * 0.3f;
        Color color;
        foreach (KeyValuePair<string, MenuState> entry in keyValuePairs)
        {
            if (m_currentSelection == entry.Value)
                if (m_awaitingInput)
                    color = Color.Yellow;
                else
                    color = Color.MediumBlue;
            else
                color = Color.White;

            top = DrawMenuItem(
                m_currentSelection == entry.Value ? m_fontMenuHover : m_fontMenu,
                entry.Key, top, color
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
        int i = (int)m_currentSelection - 1 + (int)MenuState.N;
        i %= (int)MenuState.N;
        m_currentSelection = (MenuState)i;
    }

    private void EnterPressed(GameTime gameTime, float value)
    {
        switch (m_currentSelection)
        {
            case MenuState.Thrust:
            case MenuState.RotateClockwise:
            case MenuState.RotateCounterClockwise:
                m_signalAwaitingInput = true; // Need to signal since we cant change the CommandEntry list while we're iterating through it
                break;
            case MenuState.Back:
                ChangeState(GameStateEnum.MainMenu);
                break;
            default:
                throw new NotImplementedException($"The menu does not exist: {m_currentSelection}");
        }
    }

    private void NextInput(Keys key)
    {
        List<Keys> unallowed = new() { Keys.Escape, Keys.Enter };
        if (!unallowed.Contains(key))
        {
            switch (m_currentSelection)
            {
                case MenuState.Thrust:
                    m_inputMapper.SetThrust(key);
                    break;
                case MenuState.RotateClockwise:
                    m_inputMapper.SetRotateClockwise(key);
                    break;
                case MenuState.RotateCounterClockwise:
                    m_inputMapper.SetRotateCounterClockwise(key);
                    break;
                case MenuState.Back:
                default:
                    throw new NotImplementedException($"This menu does not support configuring: {m_currentSelection}");
            }
        }
        m_signalAwaitingEnded = true;
    }

    private void Cancel(GameTime gameTime, float value)
    {
        m_signalAwaitingEnded = true;
    }
}
