using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Gallop.Live.Cutt.LiveTimelineDefine;

namespace Gallop.Live.Cutt
{
    [Serializable]
    public class LiveTimelineKeyGlobalLightData : LiveTimelineKeyWithInterpolate
    {
        // Static field
        public static readonly Color DefaultRimColor;

        // Public fields
        public Vector3 lightDir;
        public Color rimColor;
        public float rimStep;
        public float rimFeather;
        public float rimSpecRate;
        public float globalRimShadowRate;
        public bool cameraFollow;
        public float RimHorizonOffset;
        public float RimVerticalOffset;
        public float RimHorizonOffset2;
        public float RimVerticalOffset2;
        public Color rimColor2;
        public float rimStep2;
        public float rimFeather2;
        public float rimSpecRate2;
        public float globalRimShadowRate2;
        public LiveCharaPositionFlag flags;
        public string BlinkLightName;
        public int BlinkLightNameHash;
        public int BlinkLightContainerIndex;
        public LiveTimelineKeyLoopType loopType;
        public int loopCount;
        public int loopExecutedCount;
        public int loopIntervalFrame;
        public bool isPasteLoopUnit;
        public bool isChangeLoopInterpolate;

        // Properties
        public bool IsSyncBlinkLightToRimColor { get; set; }
        public bool IsSyncBlinkLightToRimColor2 { get; set; }
    }

    [Serializable]
    public class LiveTimelineKeyGlobalLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyGlobalLightData>
    {

    }

    [Serializable]
    public class LiveTimelineGlobalLightData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "GlobalLight";
        public LiveTimelineKeyGlobalLightDataList keys;
    }
}
