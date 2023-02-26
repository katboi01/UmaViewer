using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMonitorControlData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MonitorControl";

        public LiveTimelineKeyMonitorControlDataList keys = new LiveTimelineKeyMonitorControlDataList();

        public LiveTimelineMonitorControlData() : base("MonitorControl") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
