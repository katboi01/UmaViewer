using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMobCyalume3DLookAtPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Group";

        public LiveTimelineKeyMobCyalume3DLookAtPositionDataList keys = new LiveTimelineKeyMobCyalume3DLookAtPositionDataList();

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }

        public LiveTimelineMobCyalume3DLookAtPositionData() : base("Group") { }
    }
}
