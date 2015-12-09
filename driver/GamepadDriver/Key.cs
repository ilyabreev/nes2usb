using System;

namespace GamepadDriver
{
    [Flags]
    public enum Keys
    {
        Right = 1,
        Left = 2,
        Down = 4,
        Up = 8,
        Start = 16,
        Select = 32,
        B = 64,
        A = 128,
        None = Right | Left | Down | Up | Start | Select | B | A
    }
}
