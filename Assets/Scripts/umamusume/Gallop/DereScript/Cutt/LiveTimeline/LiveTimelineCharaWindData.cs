using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineCharaWindData : ILiveTimelineGroupData
    {
        public LiveCharaPosition charaPosition;

        public LiveTimelineKeyCharaWindDataList keys = new LiveTimelineKeyCharaWindDataList();

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}
