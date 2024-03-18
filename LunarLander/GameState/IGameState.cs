using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LunarLander;

public interface IGameState
{
    GameStateEnum State { get; }
    GameStateEnum NextState { get; set; }
    void Initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, IInputDevice inputDevice);
    void LoadContent(ContentManager contentManager);
    void Reload();
    GameStateEnum ProcessInput(GameTime gameTime);
    void Update(GameTime gameTime);
    void Render(GameTime gameTime);
    void RegisterKeys();
}