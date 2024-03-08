using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Lander.Input;

/// <summary>
/// Derived input device for the PC Keyboard
/// </summary>
public class KeyboardInput : IInputDevice
{
    /// <summary>
    /// Registers a callback-based command
    /// </summary>
    public void RegisterCommand(Keys key, bool keyPressOnly, CommandDelegate callback)
    {
        //
        // If already registered, remove it!
        if (m_commandEntries.ContainsKey(key))
            m_commandEntries.Remove(key);

        m_commandEntries.Add(key, new CommandEntry(key, keyPressOnly, callback));
    }

    public void UnregisterAll()
    {
        m_commandEntries.Clear();
    }

    /// <summary>
    /// Track all registered commands in this dictionary
    /// </summary>
    private readonly Dictionary<Keys, CommandEntry> m_commandEntries = new();

    /// <summary>
    /// Used to keep track of the details associated with a command
    /// </summary>
    private struct CommandEntry
    {
        public CommandEntry(Keys key, bool keyPressOnly, CommandDelegate callback)
        {
            this.key = key;
            this.keyPressOnly = keyPressOnly;
            this.callback = callback;
        }

        public Keys key;
        public bool keyPressOnly;
        public CommandDelegate callback;
    }

    /// <summary>
    /// Goes through all the registered commands and invokes the callbacks if they
    /// are active.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        KeyboardState state = Keyboard.GetState();
        foreach (CommandEntry entry in this.m_commandEntries.Values)
        {
            if (entry.keyPressOnly && KeyPressed(entry.key))
                entry.callback(gameTime, 1.0f);
            else if (!entry.keyPressOnly && state.IsKeyDown(entry.key))
                entry.callback(gameTime, 1.0f);
        }

        //
        // Move the current state to the previous state for the next time around
        m_statePrevious = state;
    }

    private KeyboardState m_statePrevious;

    /// <summary>
    /// Checks to see if a key was newly pressed
    /// </summary>
    private bool KeyPressed(Keys key)
    {
        return (Keyboard.GetState().IsKeyDown(key) && !m_statePrevious.IsKeyDown(key));
    }

    public override string ToString()
    {
        return "[ " + string.Join(", ", m_commandEntries.Keys) + " ]";
    }
}
