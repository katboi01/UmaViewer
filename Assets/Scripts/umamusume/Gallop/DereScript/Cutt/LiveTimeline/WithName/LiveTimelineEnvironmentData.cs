using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineEnvironmentData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Environment";

        public LiveTimelineKeyStageEnvironmentDataList keys = new LiveTimelineKeyStageEnvironmentDataList();

        public LiveTimelineEnvironmentData() : base("Environment")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}