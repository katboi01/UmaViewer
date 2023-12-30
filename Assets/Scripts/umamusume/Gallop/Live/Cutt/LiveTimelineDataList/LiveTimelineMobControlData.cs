using UnityEngine;

namespace Gallop.Live.Cutt
{

    [System.Serializable]
    public class LiveTimelineKeyMobControlData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 Position;
        public Vector3 Angle;
        public Vector3 Scale;
        public Quaternion Rotation;
    }


    [System.Serializable]
    public class LiveTimelineKeyMobControlDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMobControlData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMobControlData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Mob Control";
        public int GroupIndex;
        public LiveTimelineKeyMobControlDataList Keys;
    }
}
