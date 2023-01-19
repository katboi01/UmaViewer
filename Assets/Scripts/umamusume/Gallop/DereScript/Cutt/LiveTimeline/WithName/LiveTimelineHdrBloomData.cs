using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineHdrBloomData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "HdrBloom";

        public LiveTimelineKeyHdrBloomDataList keys = new LiveTimelineKeyHdrBloomDataList();

        public LiveTimelineHdrBloomData() : base("HdrBloom")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}