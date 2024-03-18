using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace LunarLander.Input;

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

    public void GetNextInput(InputCallbackDelegate callback)
    {
        m_inputCallbacks.Enqueue(callback);
    }

    private readonly Queue<InputCallbackDelegate> m_inputCallbacks = new();

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
        foreach (CommandEntry entry in m_commandEntries.Values)
        {
            if (entry.keyPressOnly && KeyPressed(state, entry.key))
                entry.callback(gameTime, 1.0f);
            else if (!entry.keyPressOnly && state.IsKeyDown(entry.key))
                entry.callback(gameTime, 1.0f);
        }

        Keys key = Keys.None;
        if (m_inputCallbacks.Count > 0)
        {
            List<Keys> keysDiff = KeyDifference(state.GetPressedKeys());
            if (keysDiff.Count > 0)
                key = keysDiff[0];

            if (key != Keys.None)
                while (m_inputCallbacks.Count > 0)
                    m_inputCallbacks.Dequeue()(key);
        }

        m_statePrevious = state;
    }

    private KeyboardState m_statePrevious;

    /// <summary>
    /// Checks to see if a key was newly pressed
    /// </summary>
    private bool KeyPressed(KeyboardState state, Keys key)
    {
        return (state.IsKeyDown(key) && !m_statePrevious.IsKeyDown(key));
    }

    private List<Keys> KeyDifference(Keys[] newKeys)
    {
        List<Keys> difference = new();
        Keys[] oldKeys = m_statePrevious.GetPressedKeys();
        foreach (Keys key in newKeys)
            if (!oldKeys.Contains(key))
                difference.Add(key);

        return difference;
    }

    public override string ToString()
    {
        return "[ " + string.Join(", ", m_commandEntries.Keys) + " ]";
    }
}
