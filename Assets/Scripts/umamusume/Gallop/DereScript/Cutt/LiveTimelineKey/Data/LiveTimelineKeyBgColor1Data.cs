using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyBgColor1Data : LiveTimelineKeyWithInterpolate
    {
        public Color color = Color.white;

        public float f32;

        public int flags;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.BgColor1;
    }
}
