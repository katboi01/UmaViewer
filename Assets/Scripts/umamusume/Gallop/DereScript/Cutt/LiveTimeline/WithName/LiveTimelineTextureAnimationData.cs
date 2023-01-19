using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineTextureAnimationData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Texture Animation";

        public LiveTimelineKeyTextureAnimationDataList keys = new LiveTimelineKeyTextureAnimationDataList();

        public LiveTimelineTextureAnimationData() : base("Texture Animation") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
