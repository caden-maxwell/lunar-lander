﻿using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LunarLander;

public class SettingsView : GameStateView
{
    private SpriteFont m_font;
    private const string MESSAGE = "Change keybinds here";
    private InputMapper m_inputMapper;

    public SettingsView(InputMapper inputMapper)
    {
        m_inputMapper = inputMapper;
    }

    public override GameStateEnum State { get; } = GameStateEnum.Settings;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.Settings;

    public override void LoadContent(ContentManager contentManager)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/menu");
    }

    public override void Reload() { }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        Vector2 stringSize = m_font.MeasureString(MESSAGE);
        m_spriteBatch.DrawString(m_font, MESSAGE,
            new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, m_graphics.PreferredBackBufferHeight / 2 - stringSize.Y), Color.Yellow);

        m_spriteBatch.End();
    }

    public override void Update(GameTime gameTime) { }
}