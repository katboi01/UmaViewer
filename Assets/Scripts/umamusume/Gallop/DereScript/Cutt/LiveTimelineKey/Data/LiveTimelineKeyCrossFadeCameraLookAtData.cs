using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCrossFadeCameraLookAtData : LiveTimelineKeyCameraLookAtData
    {
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CrossFadeCameraLookAt;

        protected override Vector3 GetValue(LiveTimelineControl timelineControl, LiveCameraLookAtType type, Vector3 camPos, bool containOffset)
        {
            Vector3 vector = Vector3.zero;
            switch (type)
            {
                case LiveCameraLookAtType.Locator:
                case LiveCameraLookAtType.Rotation:
                    return base.GetValue(timelineControl, type, camPos, containOffset);
                case LiveCameraLookAtType.Character:
                    {
                        Quaternion crossCameraWorldRotation = timelineControl.GetCrossCameraWorldRotation();
                        Vector3 extraCameraLayerOffset = timelineControl.GetExtraCameraLayerOffset(crossCameraWorldRotation);
                        vector = timelineControl.GetPositionWithCharacters(lookAtCharaPos, lookAtCharaParts, extraCameraLayerOffset);
                        break;
                    }
            }
            if (!containOffset)
            {
                return vector;
            }
            return vector + offset;
        }
    }
}

