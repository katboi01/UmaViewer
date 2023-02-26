using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineCharaMotSeqData : ILiveTimelineGroupData
    {
        public LiveTimelineKeyCharaMotionSeqDataList keys = new LiveTimelineKeyCharaMotionSeqDataList();

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}
