using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationConvert : MonoBehaviour
{
    public static Quaternion fromMaya(Vector3 euler_angle)
    {
        euler_angle.x *= Mathf.Deg2Rad;
        euler_angle.y *= Mathf.Deg2Rad;
        euler_angle.z *= Mathf.Deg2Rad;

        float c = Mathf.Cos(euler_angle[0] / 2);
        float d = Mathf.Cos(euler_angle[1] / 2);
        float e = Mathf.Cos(euler_angle[2] / 2);
        float f = Mathf.Sin(euler_angle[0] / 2);
        float g = Mathf.Sin(euler_angle[1] / 2);
        float h = Mathf.Sin(euler_angle[2] / 2);

        float x = f * d * e - c * g * h;
        float y = c * g * e + f * d * h;
        float z = c * d * h - f * g * e;
        float w = c * d * e + f * g * h;

        return new Quaternion(x, y, z, w);
    }
}
