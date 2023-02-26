using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyPropsAttachData : LiveTimelineKeyWithInterpolate
    {
        public string attachJointName = "";

        public int attachJointHash;

        public int settingFlags;

        public int propsID = -1;

        public Vector3 offsetPosition = Vector3.zero;

        public Vector3 rotation = Vector3.zero;

        public bool isAnimationChange;

        public int animationId;

        public float animationSpeed = 1f;

        public float animationOffset;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.PropsAttach;
    }
}
