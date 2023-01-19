using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineBgColor1Data : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "BgColor1";

        public LiveTimelineKeyBgColor1DataList keys = new LiveTimelineKeyBgColor1DataList();

        public LiveTimelineBgColor1Data() : base("BgColor1")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}