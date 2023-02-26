using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineColorCorrectionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "ColorCorrection";

        public LiveTimelineKeyColorCorrectionDataList keys = new LiveTimelineKeyColorCorrectionDataList();

        public LiveTimelineColorCorrectionData() : base("ColorCorrection")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}