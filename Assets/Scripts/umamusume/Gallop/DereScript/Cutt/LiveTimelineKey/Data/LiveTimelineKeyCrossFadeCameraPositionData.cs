using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCrossFadeCameraPositionData : LiveTimelineKeyCameraPositionData
    {
        public bool isEnable;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CrossFadeCameraPos;

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
                        Quaternion crossCameraWorldRotation = timelineControl.GetCrossCameraWorldRotation();
                        Vector3 extraCameraLayerOffset = timelineControl.GetExtraCameraLayerOffset(crossCameraWorldRotation);
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
