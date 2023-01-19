using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaHeightMotionData : LiveTimelineKeyCharaMotionData
    {
        public enum BlendType
        {
            Self,
            Target,
            SelfFix,
            TargetParts
        }

        [Serializable]
        public struct PartsMotion
        {
            public AnimationClip baseMotion;

            public AnimationClip blendMotion;

            public LiveCharaPosition targetPosition;

            public LiveTimelineKeyCharaIKData.IKPart positionOffset;
        }

        public bool interpolate;

        public float blendRate = 1f;

        public BlendType blendType;

        public LiveCharaPosition targetPosition;

        public LiveTimelineKeyCharaIKData.IKPart positionOffset;

        public PartsMotion[] partsMotion;

        public LiveCharaPosition target2Position;

        public float targetBlendRate;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaHeightMotion;
    }
}

