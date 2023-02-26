using UnityEngine;

namespace Cutt
{
    public class BezierCalcWork
    {
        public static BezierCalcWork cameraPos = new BezierCalcWork();

        public static BezierCalcWork cameraLookAt = new BezierCalcWork();

        private Vector3[] _points = new Vector3[17];

        public void Set(Vector3 startPos, Vector3 endPos, int bezierNum)
        {
            _points[0] = startPos;
            _points[1 + bezierNum] = endPos;
        }

        public void UpdatePoints(LiveTimelineKeyCameraPositionData posKey, LiveTimelineControl timelineControl)
        {
            posKey.GetBezierPoints(timelineControl, _points, 1);
        }

        public void UpdatePoints(LiveTimelineKeyCameraLookAtData lookAtKey, LiveTimelineControl timelineControl, Vector3 camPos)
        {
            lookAtKey.GetBezierPoints(timelineControl, camPos, _points, 1);
        }

        public void Calc(int bezierNum, float t, out Vector3 pos)
        {
            BezierUtil.Calc(_points, bezierNum + 2, t, out pos);
        }
    }
}