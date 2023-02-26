﻿using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyScreenFadeData : LiveTimelineKeyWithInterpolate
    {
        public bool onlyQuality3DLight;

        public Color color;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.ScreenFade;
    }
}
