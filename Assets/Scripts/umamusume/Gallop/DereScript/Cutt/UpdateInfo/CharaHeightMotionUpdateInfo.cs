using UnityEngine;

namespace Cutt
{
    public struct CharaHeightMotionUpdateInfo
    {
        public LiveCharaPosition selfPosition;

        public LiveTimelineKeyCharaHeightMotionData.BlendType blendType;

        public LiveTimelineKeyCharaIKData.IKPart offsetPositionEffector;

        public Vector3 offsetRate;

        public int extendNum;

        public Vector3[] offsetRateExtend;

        public LiveTimelineKeyCharaIKData.IKPart[] offsetPositionEffectorExtend;
    }
}
