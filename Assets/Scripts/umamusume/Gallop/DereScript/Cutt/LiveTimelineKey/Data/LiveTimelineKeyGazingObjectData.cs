using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyGazingObjectData : LiveTimelineKeyWithInterpolate
    {
        public LiveCharaPositionFlag lookAtCharaFlags;

        public LiveCameraLookAtCharaParts lookAtCharaParts = LiveCameraLookAtCharaParts.Waist;

        public Vector3 lookAtOffset = Vector3.zero;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.GazingObject;
    }
}
