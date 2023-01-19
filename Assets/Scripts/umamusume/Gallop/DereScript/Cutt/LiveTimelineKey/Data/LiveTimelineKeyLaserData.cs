using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyLaserData : LiveTimelineKeyWithInterpolate
    {
        [Serializable]
        public class PartsTransform
        {
            [NonSerialized]
            public bool disp = false;

            public Vector3 localPos = Vector3.zero;

            public Vector3 localRot = Vector3.zero;

            public Vector3 localScale = Vector3.one;

            public void SetLerp(PartsTransform p0, PartsTransform p1, float t)
            {
                localPos = Vector3.Lerp(p0.localPos, p1.localPos, t);
                localRot = Vector3.Lerp(p0.localRot, p1.localRot, t);
                localScale = Vector3.Lerp(p0.localScale, p1.localScale, t);
            }

            public void CopyTo(PartsTransform dst)
            {
                dst.localPos = localPos;
                dst.localRot = localRot;
                dst.localScale = localScale;
            }
        }

        public const int kAttrBlink = 65536;

        public eLaserForm formation;

        public float degRootYaw;

        public float degLaserPitch;

        public float posInterval;

        public float blinkPeriod;

        public PartsTransform rootTransform = new PartsTransform();

        public PartsTransform[] joints = new PartsTransform[5]
        {
        new PartsTransform(),
        new PartsTransform(),
        new PartsTransform(),
        new PartsTransform(),
        new PartsTransform()
        };

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Laser;

        public bool IsBlink()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrBlink);
        }
    }
}
