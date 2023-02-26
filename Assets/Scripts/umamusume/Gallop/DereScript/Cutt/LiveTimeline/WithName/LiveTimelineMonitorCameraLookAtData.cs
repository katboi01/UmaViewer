using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMonitorCameraLookAtData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MonitorCameraLookAt";

        public LiveTimelineKeyMonitorCameraLookAtDataList keys = new LiveTimelineKeyMonitorCameraLookAtDataList();

        public LiveTimelineMonitorCameraLookAtData() : base("MonitorCameraLookAt") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
