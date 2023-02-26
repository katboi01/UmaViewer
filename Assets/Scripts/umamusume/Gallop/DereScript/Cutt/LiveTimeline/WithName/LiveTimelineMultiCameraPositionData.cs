using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMultiCameraPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MultiCameraPos";

        public LiveTimelineKeyMultiCameraPositionDataList keys = new LiveTimelineKeyMultiCameraPositionDataList();

        public LiveTimelineMultiCameraPositionData() : base("MultiCameraPos") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
