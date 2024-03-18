using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LunarLander;

public abstract class GameStateView : IGameState
{
    protected GraphicsDeviceManager m_graphics;
    protected SpriteBatch m_spriteBatch;

    public abstract GameStateEnum State { get; }
    public abstract GameStateEnum NextState { get; set; }
    protected bool m_stateChanged = false;
    protected IInputDevice m_inputDevice;

    public virtual void Initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, IInputDevice inputDevice)
    {
        m_graphics = graphics;
        m_spriteBatch = new(graphicsDevice);
        m_inputDevice = inputDevice;
    }

    public virtual void RegisterKeys()
    {
        m_inputDevice.RegisterCommand(Keys.Escape, true, new CommandDelegate(EscPressed));
    }

    public abstract void Reload();

    public abstract void LoadContent(ContentManager contentManager);

    public virtual GameStateEnum ProcessInput(GameTime gameTime)
    {
        if (!m_stateChanged)
            return State;

        m_stateChanged = false;
        return NextState;
    }
    public abstract void Update(GameTime gameTime);

    public abstract void Render(GameTime gameTime);

    public virtual void EscPressed(GameTime gameTime, float value)
    {
        ChangeState(GameStateEnum.MainMenu);
    }

    protected void ChangeState(GameStateEnum nextState)
    {
        NextState = nextState;
        m_stateChanged = true;
    }
}
