using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveTimelineKeyLoopType
    {
        None = 0,
        CopyStart = 1,
        CopyEnd = 2,
        Paste = 3,
        Max = 4
    }

    [System.Serializable]
    public class ILiveTimelineGroupDataWithName : ILiveTimelineGroupData
    {
        public string name;
    }

    [System.Serializable]
    public class LiveTimelineKeyBgColor1Data : LiveTimelineKeyWithInterpolate
    {
        public Color color;
        public float power;
        public float scale;
        public int flags;
        public Color toonDarkColor;
        public Color toonBrightColor;
        public float vertexColorToonPower;
        public float outlineWidthPower;
        public Color outlineColor;
        public ModelController.OutlineColorBlend outlineColorBlend;
        public LiveTimelineKeyLoopType loopType;
        public int loopCount;
        public int loopExecutedCount;
        public int loopIntervalFrame;
        public bool isPasteLoopUnit;
        public bool isChangeLoopInterpolate;
        public bool IsProjector;
        public float f32;
        public LiveTimelineKeyLoopType _loopType;
        public int _loopCount;
        public int _loopExecutedCount;
        public int _loopIntervalFrame;
        public bool _isChangeLoopInterpolate;
    }

    [System.Serializable]
    public class LiveTimelineKeyBgColor1DataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyBgColor1Data>
    {

    }

    [System.Serializable]
    public class LiveTimelineBgColor1Data : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyBgColor1DataList keys;
    }
}
