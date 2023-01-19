using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyTransformData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 position = Vector3.zero;

        public Vector3 rotate = Vector3.zero;

        public Vector3 scale = Vector3.one;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Transform;
    }
}
