using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMonitorCameraLookAtData : LiveTimelineKeyCameraLookAtData
    {
        [NonSerialized]
        private int _monitorCameraNo;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MonitorCameraLookAt;

        public override Vector3 GetValue(LiveTimelineControl timelineControl, Vector3 camPos)
        {
            return GetValue(timelineControl, lookAtType, camPos, containOffset: true);
        }

        public Vector3 GetValue(LiveTimelineControl timelineControl, Vector3 camPos, int monitorCameraNo)
        {
            _monitorCameraNo = monitorCameraNo;
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
                        Quaternion monitorCameraWorldRotation = timelineControl.GetMonitorCameraWorldRotation(_monitorCameraNo);
                        Vector3 extraCameraLayerOffset = timelineControl.GetExtraCameraLayerOffset(monitorCameraWorldRotation);
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
