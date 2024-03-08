using Lander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lander;

public interface IGameState
{
    GameStateEnum State { get; }
    void Initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics);
    void LoadContent(ContentManager contentManager);
    GameStateEnum ProcessInput(GameTime gameTime);
    void Update(GameTime gameTime);
    void Render(GameTime gameTime);
    void RegisterKeys(KeyboardInput keyboardInput);
}