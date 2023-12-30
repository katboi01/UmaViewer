using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCyalumeControlData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 Position;
        public Vector3 Angle;
        public Vector3 Scale;
        public Quaternion Rotation;

    }

    [System.Serializable]
    public class LiveTimelineKeyCyalumeControlDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCyalumeControlData>
    {

    }

    [System.Serializable]
    public class LiveTimelineCyalumeControlData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Cyalume Control";
        public int GroupIndex;
        public LiveTimelineKeyCyalumeControlDataList Keys;
    }
}