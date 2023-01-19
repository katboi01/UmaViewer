using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyProjectorData : LiveTimelineKeyWithInterpolate
    {
        public int motionID;

        public int materialID;

        public float speed = 1f;

        public Color color1 = Color.white;

        public float power = 1f;

        public Vector3 position = Vector3.zero;

        public float rotate;

        public Vector2 rotateXZ = Vector2.zero;

        public Vector2 size = Vector2.one;

        private const int kAttrAnimContinuous = 65536;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Projector;

        public bool IsAnimContinuous()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrAnimContinuous);
        }

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}
