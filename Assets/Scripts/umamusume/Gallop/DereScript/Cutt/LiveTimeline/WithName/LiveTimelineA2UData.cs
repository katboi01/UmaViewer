using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineA2UData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "A2U";

        public LiveTimelineKeyA2UDataList keys = new LiveTimelineKeyA2UDataList();

        public LiveTimelineA2UData() : base("A2U")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}