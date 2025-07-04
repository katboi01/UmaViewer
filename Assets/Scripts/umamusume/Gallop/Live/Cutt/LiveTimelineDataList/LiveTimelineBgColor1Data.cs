using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Gallop.Live.Cutt.LiveTimelineDefine;

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

    [Serializable]
    public class LiveTimelineKeyBgColor1Data : LiveTimelineKeyWithInterpolate
    {
        // Fields
        public Color color; // 0x2c
        public float power; // 0x3c
        public float scale; // 0x40
        public int flags; // 0x44
        public ColorType ColorType; // 0x48
        public float Saturation; // 0x4c
        public Color toonDarkColor; // 0x50
        public Color toonBrightColor; // 0x60
        public float vertexColorToonPower; // 0x70
        public float outlineWidthPower; // 0x74
        public Color outlineColor; // 0x78
        public OutlineColorBlend outlineColorBlend; // 0x88
        public LightBlendMode LightBlendMode; // 0x8c
        public bool IsProjector; // 0x90
        public string BlinkLightName; // 0x98
        public int BlinkLightNameHash; // 0xa0
        public int BlinkLightContainerIndex; // 0xa4
        public float BlinkLightBrightnessPower; // 0xa8
        public bool IsAdjustedBlinkLightColor; // 0xac
        public LiveTimelineKeyLoopType loopType; // 0xb0
        public int loopCount; // 0xb4
        public int loopExecutedCount; // 0xb8
        public int loopIntervalFrame; // 0xbc
        public bool isPasteLoopUnit; // 0xc0
        public bool isChangeLoopInterpolate; // 0xc1
        public float f32; // 0xc4
        public LiveTimelineKeyLoopType _loopType; // 0xc8
        public int _loopCount; // 0xcc
        public int _loopExecutedCount; // 0xd0
        public int _loopIntervalFrame; // 0xd4
        public bool _isChangeLoopInterpolate; // 0xd8
        //private const Int32 ATTR_SILHOUETTE; // 0x0
        //private const Int32 ATTR_SYNC_BLINKLIGHT; // 0x0

        public bool IsSilhouette { get; set; }
        public bool IsSyncBlinkLight { get; set; }

    }

    [Serializable]
    public class LiveTimelineKeyBgColor1DataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyBgColor1Data>
    {

    }

    [Serializable]
    public class LiveTimelineBgColor1Data : ILiveTimelineGroupDataWithName
    {
        // Fields
        private const string DEFAULT_NAME = "BgColor1"; // 0x0
        public LiveTimelineKeyBgColor1DataList keys; // 0x28
        private int[] _targetCharaIdArray; // 0x30
        private int[] _targetDressIdArray; // 0x38

        // Properties
        public int[] TargetCharaIdArray { get; }
        public int[] TargetDressIdArray { get; }

    }

    [Serializable]
    public class ILiveTimelineGroupDataWithName : ILiveTimelineGroupData
    {
        public string name;
    }
}
