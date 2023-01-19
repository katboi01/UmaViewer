using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalume3DPositionData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 translate = Vector3.zero;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume3DPosition;
    }
}
