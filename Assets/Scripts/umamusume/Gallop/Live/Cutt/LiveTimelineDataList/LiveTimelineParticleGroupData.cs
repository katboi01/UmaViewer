using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyParticleGroupData : LiveTimelineKeyWithInterpolate
    {
        public Color lerpColor;
        public float colorLerpRate;
        public float FlickerLightRate;
        public float FlickerDarkRate;
    }

    [System.Serializable]
    public class LiveTimelineKeyParticleGroupDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyParticleGroupData>
    {

    }

    [System.Serializable]
    public class LiveTimelineParticleGroupData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyParticleGroupDataList keys;
    }
}
