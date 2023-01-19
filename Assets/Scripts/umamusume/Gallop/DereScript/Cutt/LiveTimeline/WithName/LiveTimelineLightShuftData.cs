using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineLightShuftData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "LightShuft";

        public LiveTimelineKeyLightShuftDataList keys = new LiveTimelineKeyLightShuftDataList();

        public LiveTimelineLightShuftData() : base("LightShuft")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}