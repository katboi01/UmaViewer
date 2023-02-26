using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineGlassData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Glass";

        public LiveTimelineKeyGlassDataList keys = new LiveTimelineKeyGlassDataList();

        public LiveTimelineGlassData() : base("Glass")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}