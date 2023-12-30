using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyBgColor2Data : LiveTimelineKeyWithInterpolate
    {
        public Color color1;
        public Color color2;
        public float power;
        public LiveTimelineKeyLoopType loopType;
        public int loopCount;
        public int loopExecutedCount;
        public int loopIntervalFrame;
        public bool isPasteLoopUnit;
        public bool isChangeLoopInterpolate;
        public float f32; // 0x68
        public LiveTimelineKeyLoopType _loopType;
        public int _loopCount;
        public int _loopExecutedCount;
        public int _loopIntervalFrame;
        public bool _isChangeLoopInterpolate;
    }

    [System.Serializable]
    public class LiveTimelineKeyBgColor2DataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyBgColor2Data>
    {

    }

    [System.Serializable]
    public class LiveTimelineBgColor2Data : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyBgColor2DataList keys;
    }
}
