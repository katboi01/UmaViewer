using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineGlobalLightData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "GlobalLight";

        public LiveTimelineKeyGlobalLightDataList keys = new LiveTimelineKeyGlobalLightDataList();

        public LiveTimelineGlobalLightData() : base("GlobalLight")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}