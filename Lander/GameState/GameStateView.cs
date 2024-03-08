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

    public abstract void LoadContent(ContentManager contentManager);

    public virtual GameStateEnum ProcessInput(GameTime gameTime)
    {
        if (NextState == State) return State;
        GameStateEnum nextState = NextState;
        NextState = State;

        return nextState;
    }

    public abstract void Render(GameTime gameTime);

    public abstract void Update(GameTime gameTime);

    public virtual void EscPressed(GameTime gameTime, float value)
    {
        NextState = GameStateEnum.MainMenu;
    }
}
