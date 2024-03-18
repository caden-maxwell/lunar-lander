using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace LunarLander.Input;

public enum ActionEnum
{
    Thrust,
    RotateClockwise,
    RotateCounterClockwise,
}

public class InputMapper
{
    public Dictionary<ActionEnum, Keys> KeyboardMappings { get; private set; } = new();
    public Dictionary<ActionEnum, Buttons> ControllerMappings { get; private set; } = new();

    public InputMapper()
    {
        SetDefaultMappings();
    }

    private void SetDefaultMappings()
    {
        // TODO: Get keys from persistent storage
        SetThrust(Keys.Space);
        SetRotateCounterClockwise(Keys.A);
        SetRotateClockwise(Keys.D);
    }

    public void SetThrust(Keys key)
    {
        KeyboardMappings.Remove(ActionEnum.Thrust);
        KeyboardMappings.Add(ActionEnum.Thrust, key);
    }

    public void SetRotateCounterClockwise(Keys key)
    {
        KeyboardMappings.Remove(ActionEnum.RotateCounterClockwise);
        KeyboardMappings.Add(ActionEnum.RotateCounterClockwise, key);
    }
    public void SetRotateClockwise(Keys key)
    {
        KeyboardMappings.Remove(ActionEnum.RotateClockwise);
        KeyboardMappings.Add(ActionEnum.RotateClockwise, key);
    }
}