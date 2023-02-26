using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMonitorCameraPositionData : LiveTimelineKeyCameraPositionData
    {
        public bool enable;

        public float fov = 30f;

        public float roll;

        public LiveCharaPositionFlag visibleCharaPositionFlag = LiveCharaPositionFlag.All;

        [NonSerialized]
        private int _monitorCameraNo;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MonitorCameraPos;

        public override Vector3 GetValue(LiveTimelineControl timelineControl)
        {
            return GetValue(timelineControl, setType, containOffset: true);
        }

        public Vector3 GetValue(LiveTimelineControl timelineControl, int monitorCameraNo)
        {
            _monitorCameraNo = monitorCameraNo;
            return GetValue(timelineControl, setType, containOffset: true);
        }

        protected override Vector3 GetValue(LiveTimelineControl timelineControl, LiveCameraPositionType type, bool containOffset)
        {
            Vector3 vector = Vector3.zero;
            switch (type)
            {
                case LiveCameraPositionType.Locator:
                case LiveCameraPositionType.Direct:
                    return base.GetValue(timelineControl, type, containOffset);
                case LiveCameraPositionType.Character:
                    {
                        Quaternion monitorCameraWorldRotation = timelineControl.GetMonitorCameraWorldRotation(_monitorCameraNo);
                        Vector3 extraCameraLayerOffset = timelineControl.GetExtraCameraLayerOffset(monitorCameraWorldRotation);
                        vector = timelineControl.GetPositionWithCharacters(charaRelativeBase, charaRelativeParts, extraCameraLayerOffset);
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
