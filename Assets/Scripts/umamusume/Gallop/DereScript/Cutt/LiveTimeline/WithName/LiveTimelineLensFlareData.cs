using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineLensFlareData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "LensFlare";

        public LiveTimelineKeyLensFlareDataList keys = new LiveTimelineKeyLensFlareDataList();

        public LiveTimelineLensFlareData() : base("LensFlare")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}