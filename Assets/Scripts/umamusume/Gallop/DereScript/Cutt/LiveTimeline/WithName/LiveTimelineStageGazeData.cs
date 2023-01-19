using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineStageGazeData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "StageGaze";

        public LiveTimelineKeyStageGazeDataList keys = new LiveTimelineKeyStageGazeDataList();

        public int groupNo;

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }

        public LiveTimelineStageGazeData() : base("StageGaze") { }
    }
}
