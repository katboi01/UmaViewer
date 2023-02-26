﻿using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMultiCameraMaskSettings
    {
        public int maskNum;

        public Mask[] maskData = new Mask[0];

        [Serializable]
        public class Mask
        {
            public string objectName = string.Empty;
        }
    }
}
