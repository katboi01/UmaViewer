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
        public Color[] color0Array;
        public Color[] color1Array;
        public float[] powerArray;
        public bool[] isReverseHueArray;
        public BlinkLightPattern pattern; 
        public BlinkLightColorType colorType; 
        public float powerMin; 
        public float powerMax; 
        public int loopCount;
        public float waitTime;
        public float turnOnTime;
        public float turnOffTime;
        public float keepTime;
        public float intervalTime;
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
