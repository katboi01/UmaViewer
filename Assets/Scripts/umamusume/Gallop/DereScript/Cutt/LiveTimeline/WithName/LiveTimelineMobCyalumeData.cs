using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMobCyalumeData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Group";

        public LiveTimelineKeyMobCyalumeDataList keys = new LiveTimelineKeyMobCyalumeDataList();

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }

        public LiveTimelineMobCyalumeData() : base("Group") { }
    }
}
