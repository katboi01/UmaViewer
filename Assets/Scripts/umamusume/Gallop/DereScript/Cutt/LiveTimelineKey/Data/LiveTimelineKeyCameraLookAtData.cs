using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCameraLookAtData : LiveTimelineKeyWithInterpolate
    {
        public LiveCameraLookAtType lookAtType;

        public LiveCharaPositionFlag lookAtCharaPos = LiveCharaPositionFlag.Center;

        public LiveCameraLookAtCharaParts lookAtCharaParts = LiveCameraLookAtCharaParts.ConstHeightFace;

        [NonSerialized]
        public Transform lookAtTransform;

        public string lookAtTransformName = "";

        public Vector3 rotation = Vector3.forward;

        public float eyeLength = 10f;

        public Vector3 offset = Vector3.zero;

        public Vector3[] bezierPoints;

        public float traceSpeed = 0.1f;

        public bool newBezierCalcMethod;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CameraLookAt;

        public bool necessaryToUseNewBezierCalcMethod
        {
            get
            {
                if (!newBezierCalcMethod)
                {
                    return GetBezierPointCount() > 3;
                }
                return true;
            }
        }

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
            if (lookAtType == LiveCameraLookAtType.Locator && !string.IsNullOrEmpty(lookAtTransformName))
            {
                lookAtTransform = timelineControl.FindLookAtLocator(lookAtTransformName);
                _ = lookAtTransform == null;
            }
        }

        public bool IsDelay()
        {
            return attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayEnable);
        }

        public bool IsDelayContinuous()
        {
            return attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayInherit);
        }

        public bool HasBezier()
        {
            if (bezierPoints != null)
            {
                return bezierPoints.Length != 0;
            }
            return false;
        }

        public int GetBezierPointCount()
        {
            if (!HasBezier())
            {
                return 0;
            }
            return bezierPoints.Length;
        }

        public Vector3 GetBezierPoint(int index, LiveTimelineControl timelineControl, Vector3 camPos)
        {
            if (HasBezier() && index < bezierPoints.Length)
            {
                return GetValue(timelineControl, camPos) + bezierPoints[index];
            }
            return GetValue(timelineControl, camPos) + Vector3.zero;
        }

        public void GetBezierPoints(LiveTimelineControl timelineControl, Vector3 camPos, Vector3[] outPoints, int startIndex)
        {
            if (HasBezier())
            {
                int num = Mathf.Min(outPoints.Length, bezierPoints.Length);
                Vector3 value = GetValue(timelineControl, camPos);
                for (int i = 0; i < num; i++)
                {
                    outPoints[startIndex + i] = value + bezierPoints[i];
                }
            }
        }

        public virtual Vector3 GetValue(LiveTimelineControl timelineControl, Vector3 camPos)
        {
            return GetValue(timelineControl, lookAtType, camPos, containOffset: true);
        }

        protected virtual Vector3 GetValue(LiveTimelineControl timelineControl, LiveCameraLookAtType type, Vector3 camPos, bool containOffset)
        {
            Vector3 vector = Vector3.zero;
            switch (type)
            {
                case LiveCameraLookAtType.Locator:
                    if (lookAtTransform != null)
                    {
                        vector = lookAtTransform.position;
                    }
                    break;
                case LiveCameraLookAtType.Character:
                    vector = timelineControl.GetPositionWithCharacters(lookAtCharaPos, lookAtCharaParts);
                    break;
                case LiveCameraLookAtType.Rotation:
                    {
                        Vector3 vector2 = Quaternion.Euler(rotation) * Vector3.forward * eyeLength;
                        vector = camPos + vector2;
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
