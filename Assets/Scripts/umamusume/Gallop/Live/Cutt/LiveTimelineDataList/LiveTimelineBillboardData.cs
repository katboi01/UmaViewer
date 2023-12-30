using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyBillboardData : LiveTimelineKeyWithInterpolate
    {
        public bool manualAngle;
        public Vector3 angle;
        public Quaternion manualRotation;
    }

    [System.Serializable]
    public class LiveTimelineKeyBillboardDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyBillboardData>
    {

    }

    [System.Serializable]
    public class LiveTimelineBillboardData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Billboard";
        public LiveTimelineKeyBillboardDataList keys;
    }
}

