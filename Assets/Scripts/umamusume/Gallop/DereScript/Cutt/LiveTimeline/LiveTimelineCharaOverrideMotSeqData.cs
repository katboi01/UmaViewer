using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineCharaOverrideMotSeqData : ILiveTimelineGroupData
    {
        public LiveTimelineKeyCharaOverrideMotionSeqDataList keys = new LiveTimelineKeyCharaOverrideMotionSeqDataList();

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}
