using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyFacialEyeTrackData : LiveTimelineKeyWithInterpolate
    {
        private const int kAttrUp = 65536;

        private const int kAttrDown = 131072;

        private const int kAttrNop = 262144;

        private const int kAttrWorld = 524288;

        private const int kAttrDisableAvertEye = 1048576;

        private const int kAttrDisableAvertEyeFace = 2097152;

        private const int kAttrForceAvertEye = 4194304;

        private const int kAttrForceAvertEyeFace = 8388608;

        public FacialEyeTrackTarget target;

        public float speed = 1f;

        public float upOffset = 3f;

        public float downOffset = 3f;

        public int[] targetCharaIds;

        public Vector3 position;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.FacialEyeTrack;

        public bool IsUp()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrUp);
        }

        public bool IsDown()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrDown);
        }

        public bool IsNop()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrNop);
        }
        public bool IsWorldPosition()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrWorld);
        }

        public bool IsDisableAvertEye()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrDisableAvertEye);
        }

        public bool IsDisableAvertEyeFace()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrDisableAvertEyeFace);
        }

        public bool IsForceAvertEye()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrForceAvertEye);
        }

        public bool IsForceAvertEyeFace()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrForceAvertEyeFace);
        }
    }
}
