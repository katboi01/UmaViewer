using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineShaderControlData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "ShaderControl";

        public LiveTimelineKeyShaderControlDataList keys = new LiveTimelineKeyShaderControlDataList();

        [NonSerialized]
        public int updatedKeyFrame = -1;

        public LiveTimelineShaderControlData() : base("ShaderControl") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
