using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMultiCameraLookAtData : LiveTimelineKeyCameraLookAtData
    {
        public float roll;

        [NonSerialized]
        private int _multiCameraNo;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MultiCameraLookAt;

        public override Vector3 GetValue(LiveTimelineControl timelineControl, Vector3 camPos)
        {
            return GetValue(timelineControl, lookAtType, camPos, containOffset: true);
        }

        public Vector3 GetValue(LiveTimelineControl timelineControl, Vector3 camPos, int multiCameraNo)
        {
            _multiCameraNo = multiCameraNo;
            return GetValue(timelineControl, lookAtType, camPos, containOffset: true);
        }

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
                        Quaternion multiCameraWorldRotation = timelineControl.GetMultiCameraWorldRotation(_multiCameraNo);
                        Vector3 extraCameraLayerOffset = timelineControl.GetExtraCameraLayerOffset(multiCameraWorldRotation);
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
