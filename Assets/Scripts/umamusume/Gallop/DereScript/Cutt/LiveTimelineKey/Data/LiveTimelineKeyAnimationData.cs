using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyAnimationData : LiveTimelineKeyWithInterpolate
    {
        public int animationID;

        public WrapMode wrapMode;

        public float speed = 1f;

        public float offsetTime;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Animation;
    }
}

