using Gallop.ImageEffect;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyRadialBlurData : LiveTimelineKeyWithInterpolate
    {
        public RadialBlurParam.MoveBlurType moveBlurType; // 0x30
        public Vector2 radialBlurOffset; // 0x34
        public int radialBlurDownsample; // 0x3C
        public float radialBlurStartArea; // 0x40
        public float radialBlurEndArea; // 0x44
        public float radialBlurPower; // 0x48
        public int radialBlurIteration; // 0x4C
        public Vector2 radialBlurEllipseDir; // 0x50
        public float radialBlurRollEulerAngles; // 0x58
        public float depthPowerFront; // 0x5C
        public float depthPowerBack; // 0x60
        public Vector4 depthCancelRect; // 0x64
        public float depthCancelBlendLength; // 0x74
    }


    [System.Serializable]
    public class LiveTimelineKeyRadialBlurDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyRadialBlurData>
    {

    }
}
