using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineParticleGroupData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "ParticleGroup";

        public LiveTimelineKeyParticleGroupDataList keys = new LiveTimelineKeyParticleGroupDataList();

        public LiveTimelineParticleGroupData() : base("ParticleGroup") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
