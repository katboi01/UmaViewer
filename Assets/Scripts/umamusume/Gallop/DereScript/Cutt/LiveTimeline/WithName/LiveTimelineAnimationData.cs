using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineAnimationData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Animation";

        public LiveTimelineKeyAnimationDataList keys = new LiveTimelineKeyAnimationDataList();

        public LiveTimelineAnimationData() : base("Animation")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}
