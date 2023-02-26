using System;

namespace Cutt
{
    [Flags]
    public enum LiveCharaPositionFlag
    {
        Center = 0x1,
        Left1 = 0x2,
        Right1 = 0x4,
        Left2 = 0x8,
        Right2 = 0x10,
        Left3 = 0x20,
        Right3 = 0x40,
        Left4 = 0x80,
        Right4 = 0x100,
        Left5 = 0x200,
        Right5 = 0x400,
        Left6 = 0x800,
        Right6 = 0x1000,
        Left7 = 0x2000,
        Right7 = 0x4000,
        Other = 0x7FFE,
        All = 0x7FFF
    }
}
