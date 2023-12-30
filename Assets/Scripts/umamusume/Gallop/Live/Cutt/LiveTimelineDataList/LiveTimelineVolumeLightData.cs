using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyVolumeLightData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 sunPosition; // 0x30
        public Color color1; // 0x3C
        public float power; // 0x4C
        public float komorebi; // 0x50
        public float blurRadius; // 0x54
        public float ColorRate; // 0x58
        public float ScreenColorPower; // 0x5C
        public float EffectColorPower; // 0x60
        public bool enable; // 0x64
        public bool isEnabledBorderClear; // 0x65
        private const int ATTR_BLUR_ALPHA = 65536;
    }

    [System.Serializable]
    public class LiveTimelineKeyVolumeLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyVolumeLightData>
    {

    }

    [System.Serializable]
    public class LiveTimelineVolumeLightData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyVolumeLightDataList keys;
    }
}
