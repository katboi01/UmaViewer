using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineProjectorData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Projector";

        public LiveTimelineKeyProjectorDataList keys = new LiveTimelineKeyProjectorDataList();

        public LiveTimelineProjectorData() : base("Projector") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
