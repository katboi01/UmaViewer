using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineDressChangeData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Glass";

        public LiveTimelineKeyGlassDataList keys = new LiveTimelineKeyGlassDataList();

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }

        public LiveTimelineDressChangeData() : base("Glass") { }
    }
}
