using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyChromaticAberrationData : LiveTimelineKeyWithInterpolate
    {
        public enum ChromaticAberrationType
        {
            Normal = 0,
            Circle = 1,
            CircleClipDirect = 2
        }

        public bool isEnable; // 0x30
        public Vector2 redOffset; // 0x34
        public Vector2 greenOffset; // 0x3C
        public Vector2 blueOffset; // 0x44
        public float power; // 0x4C
        public float clip; // 0x50
        public LiveTimelineKeyChromaticAberrationData.ChromaticAberrationType effectType; // 0x54
    }

    [System.Serializable]
    public class LiveTimelineKeyChromaticAberrationDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyChromaticAberrationData>
    {

    }

    [System.Serializable]
    public class LiveTimelineChromaticAberrationData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Chromatic Aberration";
        public LiveTimelineKeyChromaticAberrationDataList keys;
    }
}