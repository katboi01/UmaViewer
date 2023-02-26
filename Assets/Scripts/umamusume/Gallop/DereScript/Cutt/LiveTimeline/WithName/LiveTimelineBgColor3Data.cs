using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineBgColor3Data : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "BgColor3";

        public LiveTimelineKeyBgColor3DataList keys = new LiveTimelineKeyBgColor3DataList();

        public LiveTimelineBgColor3Data() : base("BgColor3")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}