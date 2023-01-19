using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMultiCameraPositionData : LiveTimelineKeyCameraPositionData
    {
        public bool enableMultiCamera;

        public float fov = 20f;

        public int maskIndex;

        public Vector3 maskPosition;

        public Vector3 maskAngle;

        public Vector3 maskScale = Vector3.one;

        [NonSerialized]
        private int _multiCameraNo;

        [NonSerialized]
        public Quaternion maskRotation;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MultiCameraPos;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
            maskRotation = Quaternion.Euler(maskAngle);
        }

        public override Vector3 GetValue(LiveTimelineControl timelineControl)
        {
            return GetValue(timelineControl, setType, containOffset: true);
        }

        public Vector3 GetValue(LiveTimelineControl timelineControl, int multiCameraNo)
        {
            _multiCameraNo = multiCameraNo;
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
                        Quaternion multiCameraWorldRotation = timelineControl.GetMultiCameraWorldRotation(_multiCameraNo);
                        Vector3 extraCameraLayerOffset = timelineControl.GetExtraCameraLayerOffset(multiCameraWorldRotation);
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
