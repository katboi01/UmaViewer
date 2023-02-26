using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyBgColor3Data : LiveTimelineKeyWithInterpolate
    {
        public Color[] _colorArray;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.BgColor3;
    }
}
