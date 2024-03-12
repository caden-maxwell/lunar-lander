using Lander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lander;

public abstract class GameStateView : IGameState
{
    protected GraphicsDeviceManager m_graphics;
    protected SpriteBatch m_spriteBatch;
    protected bool m_stateChanged = false;

    public abstract GameStateEnum State { get; }
    public abstract GameStateEnum NextState { get; set; }

    public virtual void Initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics)
    {
        m_graphics = graphics;
        m_spriteBatch = new(graphicsDevice);
    }

    public virtual void RegisterKeys(KeyboardInput keyboardInput)
    {
        keyboardInput.RegisterCommand(Keys.Escape, true, new CommandDelegate(EscPressed));
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
