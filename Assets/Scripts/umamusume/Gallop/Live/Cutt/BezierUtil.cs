using System;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public static class BezierUtil
    {
        private static Vector3[] subPointsWork = new Vector3[17];

        public static void Bezier(ref Vector3 startPos, Vector3[] points, ref Vector3 endPos, float t, out Vector3 outPos)
        {
            points[0] = startPos;
            Array.Copy(points, 0, subPointsWork, 1, points.Length);
            points[points.Length + 1] = endPos;
            outPos = CalcRecurse(points, points.Length, t);
        }

        public static void Calc(Vector3[] points, int pointNum, float t, out Vector3 outPos)
        {
            outPos = CalcRecurse(points, pointNum, t);
        }

        private static Vector3 CalcRecurse(Vector3[] points, int pointNum, float t)
        {
            if (pointNum == 2)
            {
                return Vector3.LerpUnclamped(points[0], points[1], t);
            }
            for (int i = 0; i < pointNum - 1; i++)
            {
                subPointsWork[i] = Vector3.LerpUnclamped(points[i], points[i + 1], t);
            }
            return CalcRecurse(subPointsWork, pointNum - 1, t);
        }

        public static void Calc(ref Vector3 start, ref Vector3 end, ref Vector3 cp, float t, out Vector3 v3)
        {
            float num = 1f - t;
            float num2 = t * t;
            float num3 = 2f * t * num;
            float num4 = num * num;
            v3.x = num2 * end.x + num3 * cp.x + num4 * start.x;
            v3.y = num2 * end.y + num3 * cp.y + num4 * start.y;
            v3.z = num2 * end.z + num3 * cp.z + num4 * start.z;
        }

        public static void Calc(ref Vector3 start, ref Vector3 end, ref Vector3 cp1, ref Vector3 cp2, float t, out Vector3 v3)
        {
            float num = 1f - t;
            float num2 = t * t * t;
            float num3 = 3f * t * t * num;
            float num4 = 3f * t * num * num;
            float num5 = num * num * num;
            v3.x = num2 * end.x + num3 * cp2.x + num4 * cp1.x + num5 * start.x;
            v3.y = num2 * end.y + num3 * cp2.y + num4 * cp1.y + num5 * start.y;
            v3.z = num2 * end.z + num3 * cp2.z + num4 * cp1.z + num5 * start.z;
        }

        public static void Calc(ref Vector3 start, ref Vector3 end, ref Vector3 cp1, ref Vector3 cp2, ref Vector3 cp3, float t, out Vector3 v3)
        {
            float num = 1f - t;
            float num2 = t * t * t * t;
            float num3 = 4f * t * t * t * num;
            float num4 = 6f * t * t * num * num;
            float num5 = 4f * t * num * num * num;
            float num6 = num * num * num * num;
            v3.x = num2 * end.x + num3 * cp3.x + num4 * cp2.x + num5 * cp1.x + num6 * start.x;
            v3.y = num2 * end.y + num3 * cp3.y + num4 * cp2.y + num5 * cp1.y + num6 * start.y;
            v3.z = num2 * end.z + num3 * cp3.z + num4 * cp2.z + num5 * cp1.z + num6 * start.z;
        }

        public static void Calc(ref Vector3 start, ref Vector3 end, ref Vector3 cp1, ref Vector3 cp2, ref Vector3 cp3, ref Vector3 cp4, float t, out Vector3 v3)
        {
            float num = 1f - t;
            float num2 = t * t * t * t * t;
            float num3 = 5f * t * t * t * t * num;
            float num4 = 10f * t * t * t * num * num;
            float num5 = 10f * t * t * num * num * num;
            float num6 = 5f * t * num * num * num * num;
            float num7 = num * num * num * num * num;
            v3.x = num2 * end.x + num3 * cp4.x + num4 * cp3.x + num5 * cp2.x + num6 * cp1.x + num7 * start.x;
            v3.y = num2 * end.y + num3 * cp4.y + num4 * cp3.y + num5 * cp2.y + num6 * cp1.y + num7 * start.y;
            v3.z = num2 * end.z + num3 * cp4.z + num4 * cp3.z + num5 * cp2.z + num6 * cp1.z + num7 * start.z;
        }
    }
}
