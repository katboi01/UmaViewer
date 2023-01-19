using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineBgColor2Data : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "BgColor2";

        public LiveTimelineKeyBgColor2DataList keys = new LiveTimelineKeyBgColor2DataList();

        public int groupNo = -1;

        public LiveTimelineBgColor2Data() : base("BgColor2")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}