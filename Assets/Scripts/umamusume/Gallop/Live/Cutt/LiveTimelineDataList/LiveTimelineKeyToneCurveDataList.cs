using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyToneCurveData : LiveTimelineKeyWithInterpolate
    {
        public bool IsEnable;
        public float DepthMask;
        public AnimationCurve ToneAnimationCurve;
        public Color MinCorrectionLevel;
        public Color MaxCorrectionLevel;
        public AnimationCurve MaskToneCurve;
        public Color MaskMinCorrectionLevel;
        public Color MaskMaxCorrectionLevel;
    }


    [System.Serializable]
    public class LiveTimelineKeyToneCurveDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyToneCurveData>
    {

    }
}
