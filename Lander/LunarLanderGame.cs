using Lander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lander;

public class LunarLanderGame : Game
{
    private readonly GraphicsDeviceManager m_graphics;
    private IGameState m_prevState;
    private IGameState m_currentState;
    private Dictionary<GameStateEnum, IGameState> m_states;
    private KeyboardInput m_keyboardInput;

    public LunarLanderGame()
    {
        m_graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        m_graphics.PreferredBackBufferWidth = 1920;
        m_graphics.PreferredBackBufferHeight = 1080;

        m_graphics.ApplyChanges();

        m_keyboardInput = new();

        m_states = new Dictionary<GameStateEnum, IGameState>
        {
            { GameStateEnum.MainMenu, new MainMenuView() },
            { GameStateEnum.GamePlay, new GamePlayView() },
            { GameStateEnum.HighScores, new HighScoresView() },
            { GameStateEnum.About, new AboutView() },
            { GameStateEnum.Settings, new SettingsView() }
        };

        // Init each game state
        foreach (var item in m_states)
            item.Value.Initialize(this.GraphicsDevice, m_graphics);

        // Start with main menu
        m_currentState = m_states[GameStateEnum.MainMenu];
        m_prevState = m_currentState;
        m_states[GameStateEnum.MainMenu].RegisterKeys(m_keyboardInput);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        foreach (var item in m_states)
            item.Value.LoadContent(this.Content);
    }

    protected GameStateEnum ProcessInput(GameTime gameTime)
    {
        m_keyboardInput.Update(gameTime);
        return m_currentState.ProcessInput(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        GameStateEnum nextStateEnum = ProcessInput(gameTime);

        // Special case for exiting the game
        if (nextStateEnum == GameStateEnum.Exit)
            Exit();
        else
        {
            m_currentState.Update(gameTime);
            m_prevState = m_currentState;
            m_currentState = m_states[nextStateEnum];
        }

        if (m_currentState.State != m_prevState.State)
        {
            m_keyboardInput.UnregisterAll();
            m_currentState.RegisterKeys(m_keyboardInput);
            Debug.WriteLine($"{m_currentState.State}: {m_keyboardInput}");
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        m_currentState.Render(gameTime);

        base.Draw(gameTime);
    }
}
