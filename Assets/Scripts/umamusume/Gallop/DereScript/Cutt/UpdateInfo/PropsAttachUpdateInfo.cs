using UnityEngine;

namespace Cutt
{
    public struct PropsAttachUpdateInfo
    {
        public float progressTime;

        public LiveTimelinePropsAttachData data;

        public Vector3 offsetPosition;

        public Vector3 rotation;

        public int settingFlags;

        public int propsID;

        public int attachJointHash;

        public int animationId;

        public float animationSpeed;

        public float animationOffset;

        public bool changeAnimation;
    }
}
