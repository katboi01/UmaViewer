using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMobCyalume3DPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Group";

        public LiveTimelineKeyMobCyalume3DPositionDataList keys = new LiveTimelineKeyMobCyalume3DPositionDataList();

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }

        public LiveTimelineMobCyalume3DPositionData() : base("Group") { }
    }
}
