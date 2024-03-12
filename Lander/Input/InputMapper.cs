using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Lander.Input;

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
        KeyboardMappings.Add(ActionEnum.Thrust, Keys.W);
        KeyboardMappings.Add(ActionEnum.RotateCounterClockwise, Keys.A);
        KeyboardMappings.Add(ActionEnum.RotateClockwise, Keys.D);
    }
}