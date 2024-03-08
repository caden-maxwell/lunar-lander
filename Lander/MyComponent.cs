
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Lander;

class MyComponent : GameComponent
{
    public MyComponent(Game game) : base(game) { }

    public override void Update(GameTime gameTime)
    {
        Debug.WriteLine("GameComponent Updated", gameTime);

        base.Update(gameTime);
    }
}
