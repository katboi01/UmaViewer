using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveCameraLookAtType
    {
        Direct = 0,
        Character = 1
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraLookAtData : LiveTimelineKeyWithInterpolate
    {
        public LiveCameraLookAtType lookAtType;
        public Vector3 position;
        public LiveCharaPositionFlag lookAtCharaPos;
        public LiveCameraCharaParts lookAtCharaParts;
        public Vector3 charaPos;
        public Vector3[] bezierPoints;
        public float traceSpeed;

        public Vector3 rotation = Vector3.forward;

        public float eyeLength = 10f;

        public Vector3 offset = Vector3.zero;

        public bool newBezierCalcMethod;

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
            Vector3 vector = position;
            switch (type)
            {
                case LiveCameraLookAtType.Character:
                    vector += timelineControl.GetPositionWithCharacters(lookAtCharaPos, lookAtCharaParts, charaPos);
                    break;
                case LiveCameraLookAtType.Direct:
                    {
                        //Vector3 vector2 = Quaternion.Euler(rotation) * Vector3.forward * eyeLength;
                        //vector = camPos + vector2;
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

    [System.Serializable]
    public class LiveTimelineKeyCameraLookAtDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraLookAtData>
    {

    }
}

