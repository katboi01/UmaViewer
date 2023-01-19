using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineCharaHeightMotSeqData : ILiveTimelineGroupData
    {
        public LiveCharaPosition charaPosition;

        public LiveTimelineKeyCharaHeightMotionSeqDataList keys = new LiveTimelineKeyCharaHeightMotionSeqDataList();

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}
