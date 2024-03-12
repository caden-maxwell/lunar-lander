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
    public Dictionary<ActionEnum, Keys> KeyboardMappings { get; set; } = new();
    public Dictionary<ActionEnum, Buttons> ControllerMappings { get; set; } = new();

    public InputMapper()
    {
        SetDefaultMappings();
    }

    private void SetDefaultMappings()
    {
        // TODO: Get keys from persistent storage
        KeyboardMappings.Add(ActionEnum.Thrust, Keys.Space);
        KeyboardMappings.Add(ActionEnum.RotateCounterClockwise, Keys.A);
        KeyboardMappings.Add(ActionEnum.RotateClockwise, Keys.D);
    }
}