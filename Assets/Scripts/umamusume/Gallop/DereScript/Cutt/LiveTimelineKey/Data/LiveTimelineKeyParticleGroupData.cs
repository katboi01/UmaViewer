using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyParticleGroupData : LiveTimelineKeyWithInterpolate
    {
        public Color lerpColor = Color.white;

        public float colorLerpRate;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.ParticleGroup;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}
