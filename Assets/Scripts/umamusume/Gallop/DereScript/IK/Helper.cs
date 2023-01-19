using UnityEngine;

namespace IK
{
    public static class Helper
    {
        public static readonly Vector3 AxisRight = Vector3.right;

        public static readonly Vector3 AxisUp = Vector3.up;

        public static readonly Vector3 AxisForward = Vector3.forward;

        public const float SqrEpsilon = 1E-12f;

        public const float DotSmall = 0.999999f;

        public static readonly Quaternion QuaternionIdentity = Quaternion.identity;

        public static Quaternion MakeQuaternion(float x, float y, float z)
        {
            float w = Mathf.Cos(x / 2f);
            float x2 = Mathf.Sin(x / 2f);
            float w2 = Mathf.Cos(y / 2f);
            float y2 = Mathf.Sin(y / 2f);
            float w3 = Mathf.Cos(z / 2f);
            float z2 = Mathf.Sin(z / 2f);
            Quaternion quaternion = default(Quaternion);
            quaternion.x = x2;
            quaternion.y = 0f;
            quaternion.z = 0f;
            quaternion.w = w;
            Quaternion quaternion2 = default(Quaternion);
            quaternion2.x = 0f;
            quaternion2.y = y2;
            quaternion2.z = 0f;
            quaternion2.w = w2;
            Quaternion quaternion3 = default(Quaternion);
            quaternion3.x = 0f;
            quaternion3.y = 0f;
            quaternion3.z = z2;
            quaternion3.w = w3;
            return quaternion2 * quaternion * quaternion3;
        }

        public static void FindBestAxisVectors(Vector3 dir, out Vector3 axis1, out Vector3 axis2)
        {
            float num = Mathf.Abs(dir.x);
            float num2 = Mathf.Abs(dir.y);
            float num3 = Mathf.Abs(dir.z);
            if (num2 > num && num2 > num3)
            {
                axis1 = AxisRight;
            }
            else
            {
                axis1 = AxisUp;
            }
            axis1 = Vector3.Normalize(axis1 - dir * Vector3.Dot(axis1, dir));
            axis2 = Vector3.Cross(axis1, dir);
        }
    }
}
