using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalume3DLookAtPositionData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 translate = Vector3.zero;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume3DLookAtPosition;
    }
}
