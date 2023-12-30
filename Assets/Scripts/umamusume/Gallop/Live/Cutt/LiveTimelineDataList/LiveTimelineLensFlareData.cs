using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyLensFlareData : LiveTimelineKeyWithInterpolate
    {
        private const int ATTRIBUTE_BIT_SCREEN_FIT = 65536;
        public Vector3 offset;
        public Color color;
        public float brightness;
        public float fadeSpeed;
        public bool enableParameter;
        public bool enableFlare;
        public bool IsAutoBrightness;
        public bool IsOverridePosition;
    }

    [System.Serializable]
    public class LiveTimelineKeyLensFlareDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyLensFlareData>
    {

    }

    [System.Serializable]
    public class LiveTimelineLensFlareData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "LensFlare";
        public LiveTimelineKeyLensFlareDataList keys;
    }
}
