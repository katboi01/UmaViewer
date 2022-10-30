using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gallop.Cyalume;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyAudienceData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 position;
        public Vector3 rotate;
        public Vector3 scale;
        public Color cyalumeColor;
        public Color cyalumeGlowColor;
        public float cyalumeGlowColorPower;
        public float cyalumeMaskRadius;
        public AudienceController.AnimationSetting animationSetting;
        public int animationRootIndex;
        public AudienceController.AnimationBodyRegion animationBodyRegion;
        public AudienceController.AnimationCategory animationCategory;
        public int animationIndex;
        public WrapMode animationWrapMode;
        public float animationSpeed;
        public float animationOffsetTime;
    }

    [System.Serializable]
    public class LiveTimelineKeyAudienceDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyAudienceData>
    {

    }

    [System.Serializable]
    public class LiveTimelineAudienceData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyAudienceDataList keys;
        [SerializeField]
        private int _objectIndex;
    }
}
