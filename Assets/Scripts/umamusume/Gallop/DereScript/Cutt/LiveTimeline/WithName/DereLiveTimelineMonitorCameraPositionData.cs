using System;

namespace Cutt
{
    [Serializable]
    public class DereLiveTimelineMonitorCameraPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MonitorCameraPos";

        public DereLiveTimelineKeyMonitorCameraPositionDataList keys = new DereLiveTimelineKeyMonitorCameraPositionDataList();

        public DereLiveTimelineMonitorCameraPositionData() : base("MonitorCameraPos") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
