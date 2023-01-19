using System;

namespace Cutt
{
    [Flags]
    public enum LiveTimelineKeyAttribute
    {
        Disable = 0x1,
        CameraDelayEnable = 0x2,
        CameraDelayInherit = 0x4,
        KeyCommonBitMask = 0xFFFF
    }
}
