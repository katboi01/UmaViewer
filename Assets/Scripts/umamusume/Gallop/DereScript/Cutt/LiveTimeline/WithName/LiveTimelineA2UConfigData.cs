using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineA2UConfigData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "A2UConfig";

        public LiveTimelineKeyA2UConfigDataList keys = new LiveTimelineKeyA2UConfigDataList();

        public LiveTimelineA2UConfigData() : base("A2UConfig")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}