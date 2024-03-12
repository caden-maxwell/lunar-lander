using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LunarLander.Input;

public delegate void CommandDelegate(GameTime gameTime, float value);
public delegate void CommandDelegatePosition(GameTime GameTime, int x, int y);

/// <summary>
/// Abstract base class that defines how input is presented to game code.
/// </summary>
public interface IInputDevice
{
    void Update(GameTime gameTime);
    void RegisterCommand(Keys key, bool keyPressOnly, CommandDelegate callback);
    void UnregisterAll();
}
