using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineVolumeLightData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "SunShafts";

        public LiveTimelineKeyVolumeLightDataList keys = new LiveTimelineKeyVolumeLightDataList();

        public LiveTimelineVolumeLightData() : base("SunShafts") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
