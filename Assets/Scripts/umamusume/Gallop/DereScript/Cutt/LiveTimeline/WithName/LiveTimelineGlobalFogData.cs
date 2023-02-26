using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineGlobalFogData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "GlobalFog";

        public LiveTimelineKeyGlobalFogDataList keys = new LiveTimelineKeyGlobalFogDataList();

        public LiveTimelineGlobalFogData() : base("GlobalFog")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}