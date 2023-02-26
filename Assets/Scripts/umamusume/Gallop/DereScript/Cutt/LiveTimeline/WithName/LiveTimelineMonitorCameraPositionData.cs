using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMonitorCameraPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MonitorCameraPos";

        public LiveTimelineKeyMonitorCameraPositionDataList keys = new LiveTimelineKeyMonitorCameraPositionDataList();

        public LiveTimelineMonitorCameraPositionData() : base("MonitorCameraPos") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
