using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Gallop.Live.Cutt.LiveTimelineDefine;

namespace Gallop.Live.Cutt
{
    [Serializable]
    public class LiveTimelineKeyBgColor2Data : LiveTimelineKeyWithInterpolate
    {
        // Fields
        public Color color1; // 0x2c
        public Color color2; // 0x3c
        public float power; // 0x4c
        public string BlinkLightName; // 0x50
        public int BlinkLightNameHash; // 0x58
        public int BlinkLightContainerIndex; // 0x5c
        public float BlinkLightBrightnessPower; // 0x60
        public bool IsSyncBlinkLightToColor1; // 0x64
        public bool IsSyncBlinkLightToColor2; // 0x65
        public bool IsAdjustedBlinkLightColor; // 0x66
        public LiveTimelineKeyLoopType loopType; // 0x68
        public int loopCount; // 0x6c
        public int loopExecutedCount; // 0x70
        public int loopIntervalFrame; // 0x74
        public bool isPasteLoopUnit; // 0x78
        public bool isChangeLoopInterpolate; // 0x79
        public float f32; // 0x7c
        public LiveTimelineKeyLoopType _loopType; // 0x80
        public int _loopCount; // 0x84
        public int _loopExecutedCount; // 0x88
        public int _loopIntervalFrame; // 0x8c
        public bool _isChangeLoopInterpolate; // 0x90

    }

    [Serializable]
    public class LiveTimelineKeyBgColor2DataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyBgColor2Data>
    {

    }

    [Serializable]
    public class LiveTimelineBgColor2Data : ILiveTimelineGroupDataWithName
    {
        // Fields
        private const string DEFAULT_NAME = "BgColor2"; // 0x0
        public LiveTimelineKeyBgColor1DataList keys; // 0x28
    }
}
