using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineRendererData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Renderer Object";

        public LiveTimelineKeyRendererDataList keys = new LiveTimelineKeyRendererDataList();

        public LiveTimelineRendererData() : base("Renderer Object") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
