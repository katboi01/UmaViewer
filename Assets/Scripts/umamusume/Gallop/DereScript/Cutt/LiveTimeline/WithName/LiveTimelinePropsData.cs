using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelinePropsData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Props";

        public LiveTimelineKeyPropsDataList keys = new LiveTimelineKeyPropsDataList();

        public LiveTimelinePropsData() : base("Props") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
