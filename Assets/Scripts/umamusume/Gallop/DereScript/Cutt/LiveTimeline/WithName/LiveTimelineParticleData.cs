using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineParticleData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Particle";

        public LiveTimelineKeyParticleDataList keys = new LiveTimelineKeyParticleDataList();

        public LiveTimelineParticleData() : base("Particle") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
