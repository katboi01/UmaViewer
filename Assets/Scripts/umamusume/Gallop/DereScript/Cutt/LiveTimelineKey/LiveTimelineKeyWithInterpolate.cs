using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public abstract class LiveTimelineKeyWithInterpolate : LiveTimelineKey
    {
        public LiveCameraInterpolateType interpolateType;

        public AnimationCurve curve;

        public LiveTimelineEasing.Type easingType;

        public override bool IsInterpolateKey()
        {
            return interpolateType != LiveCameraInterpolateType.None;
        }
    }
}
