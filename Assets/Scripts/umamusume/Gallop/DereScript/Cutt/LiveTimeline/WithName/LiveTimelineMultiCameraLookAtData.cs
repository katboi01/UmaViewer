using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMultiCameraLookAtData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MultiCameraLookAt";

        public LiveTimelineKeyMultiCameraLookAtDataList keys = new LiveTimelineKeyMultiCameraLookAtDataList();

        public LiveTimelineMultiCameraLookAtData() : base("MultiCameraLookAt") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
