using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum BlinkLightPattern
    {
        None = 0,
        Random = 1,
        Ascend = 2,
        Descend = 3,
        Max = 4
    }

    public enum BlinkLightColorType
    {
        Blend = 0,
        Ascend = 1,
        Descend = 2,
        Switch = 3,
        Max = 4
    }

    [System.Serializable]
    public class LiveTimelineKeyBlinkLightData : LiveTimelineKeyWithInterpolate
    {
        public LiveDefine.LightBlendMode LightBlendMode; // 0x30
        public Color[] color0Array; // 0x38
        public Color[] color1Array; // 0x40
        public float[] powerArray; // 0x48
        public bool[] isReverseHueArray; // 0x50
        public BlinkLightPattern pattern; // 0x58
        public BlinkLightColorType colorType; // 0x5C
        public float powerMin; // 0x60
        public float powerMax; // 0x64
        public int loopCount; // 0x68
        public float waitTime; // 0x6C
        public float turnOnTime; // 0x70
        public float turnOffTime; // 0x74
        public float keepTime; // 0x78
        public float intervalTime; // 0x7C
    }

    [System.Serializable]
    public class LiveTimelineKeyBlinkLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyBlinkLightData>
    {

    }

    [System.Serializable]
    public class LiveTimelineBlinkLightData : ILiveTimelineGroupDataWithName
    {
        private const string DEFAULT_NAME = "BlinkLight";
        public LiveTimelineKeyBlinkLightDataList keys;
    }
}
