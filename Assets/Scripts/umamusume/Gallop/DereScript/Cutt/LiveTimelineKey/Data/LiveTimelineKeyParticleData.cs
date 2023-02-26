using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyParticleData : LiveTimelineKeyWithInterpolate
    {
        public float emissionRate = 4.5f;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Particle;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}
