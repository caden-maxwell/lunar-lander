using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace LunarLander;

public enum SpaceBodiesEnum
{
    Sun,
    Mercury,
    Venus,
    Earth,
    Moon,
    Mars,
    Jupiter,
    Titan,
    Saturn,
    Uranus,
    Neptune,
    Pluto
}

public class LunarLanderGame : Game
{
    private readonly GraphicsDeviceManager m_graphics;
    private IGameState m_prevState;
    private IGameState m_currentState;
    private Dictionary<GameStateEnum, IGameState> m_states;
    private IInputDevice m_inputDevice;
    private InputMapper m_inputMapper = new();
    private const GameStateEnum m_startState = GameStateEnum.GamePlay;

    public LunarLanderGame()
    {
        m_graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        m_graphics.PreferredBackBufferWidth = 1600;
        m_graphics.PreferredBackBufferHeight = 900;

        m_graphics.ApplyChanges();

        m_inputDevice = new KeyboardInput(); // This line could ideally be any input

        m_states = new Dictionary<GameStateEnum, IGameState>
        {
            { GameStateEnum.MainMenu, new MainMenuView() },
            { GameStateEnum.GamePlay, new GamePlayView(m_inputMapper, SpaceBodiesEnum.Moon) },
            { GameStateEnum.HighScores, new HighScoresView() },
            { GameStateEnum.Credits, new CreditsView() },
            { GameStateEnum.Settings, new SettingsView(m_inputMapper) }
        };

        // Init each game state
        foreach (var item in m_states)
            item.Value.Initialize(this.GraphicsDevice, m_graphics);

        // Start with game play state for debugging purposes
        m_currentState = m_states[m_startState];
        m_prevState = m_currentState;
        m_currentState.RegisterKeys(m_inputDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        foreach (var item in m_states)
            item.Value.LoadContent(this.Content);
    }

    protected GameStateEnum ProcessInput(GameTime gameTime)
    {
        m_inputDevice.Update(gameTime);
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
            m_inputDevice.UnregisterAll();
            m_currentState.RegisterKeys(m_inputDevice);
            m_currentState.Reload();

            Debug.WriteLine($"{m_currentState.State}: {m_inputDevice}");
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
