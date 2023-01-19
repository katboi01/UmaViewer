using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineLaserData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Laser";

        public LiveTimelineKeyLaserDataList keys = new LiveTimelineKeyLaserDataList();

        public LiveTimelineLaserData() : base("Laser")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}