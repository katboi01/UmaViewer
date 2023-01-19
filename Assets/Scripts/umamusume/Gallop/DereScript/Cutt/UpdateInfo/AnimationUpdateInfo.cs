using UnityEngine;

namespace Cutt
{
    public struct AnimationUpdateInfo
    {
        public float progressTime;

        public LiveTimelineAnimationData data;

        public int animationID;

        public WrapMode wrapMode;

        public float speed;

        public float offsetTime;
    }
}
