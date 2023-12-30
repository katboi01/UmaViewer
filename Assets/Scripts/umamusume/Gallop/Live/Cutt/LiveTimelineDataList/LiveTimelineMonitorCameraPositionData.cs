namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMonitorCameraPositionData : LiveTimelineKeyCameraPositionData
    {
        public bool enable;
        public float fov;
        public float roll;
    }

    [System.Serializable]
    public class LiveTimelineKeyMonitorCameraPositionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMonitorCameraPositionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMonitorCameraPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MonitorCameraPos";
        public LiveTimelineKeyMonitorCameraPositionDataList keys;
    }
}
