using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace LibMMD.Util
{
    public static class MathUtil
    {
        public static Matrix4x4 Matrix4X4MuliplyFloat(Matrix4x4 mat, float val)
        {
            return new Matrix4x4
            {
                m00 = mat.m00 * val,
                m01 = mat.m01 * val,
                m02 = mat.m02 * val,
                m03 = mat.m03 * val,
                m10 = mat.m10 * val,
                m11 = mat.m11 * val,
                m12 = mat.m12 * val,
                m13 = mat.m13 * val,
                m20 = mat.m20 * val,
                m21 = mat.m21 * val,
                m22 = mat.m22 * val,
                m23 = mat.m23 * val,
                m30 = mat.m30 * val,
                m31 = mat.m31 * val,
                m32 = mat.m32 * val,
                m33 = mat.m33 * val
            };
        }

        public static Matrix4x4 Matrix4X4Add(Matrix4x4 mat1, Matrix4x4 mat2)
        {
            return new Matrix4x4
            {
                m00 = mat1.m00 + mat2.m00,
                m01 = mat1.m01 + mat2.m01,
                m02 = mat1.m02 + mat2.m02,
                m03 = mat1.m03 + mat2.m03,
                m10 = mat1.m10 + mat2.m10,
                m11 = mat1.m11 + mat2.m11,
                m12 = mat1.m12 + mat2.m12,
                m13 = mat1.m13 + mat2.m13,
                m20 = mat1.m20 + mat2.m20,
                m21 = mat1.m21 + mat2.m21,
                m22 = mat1.m22 + mat2.m22,
                m23 = mat1.m23 + mat2.m23,
                m30 = mat1.m30 + mat2.m30,
                m31 = mat1.m31 + mat2.m31,
                m32 = mat1.m32 + mat2.m32,
                m33 = mat1.m33 + mat2.m33
            };
        }

        public static Matrix4x4 QuaternionToMatrix4X4(Quaternion quaternion)
        {
            return Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one);
        }

        public static Vector3 GetTransFromMatrix4X4(Matrix4x4 mat)
        {
            return new Vector3(mat.m03, mat.m13, mat.m23);
        }

        public static void SetTransToMatrix4X4(Vector3 trans, ref Matrix4x4 mat)
        {
            mat.m03 = trans[0];
            mat.m13 = trans[1];
            mat.m23 = trans[2];
        }

        public static Vector3 Matrix4x4ColDowngrade(Matrix4x4 mat, int col)
        {
            return new Vector3(mat[0, col], mat[1, col], mat[2, col]);
        }

        public static Vector3 QuaternionToXyz(Quaternion quaternion)
        {
            var ii = quaternion.x * quaternion.x;
            var jj = quaternion.y * quaternion.y;
            var kk = quaternion.z * quaternion.z;
            var ei = quaternion.w * quaternion.x;
            var ej = quaternion.w * quaternion.y;
            var ek = quaternion.w * quaternion.z;
            var ij = quaternion.x * quaternion.y;
            var ik = quaternion.x * quaternion.z;
            var jk = quaternion.y * quaternion.z;
            Vector3 result;
            result.x = (float) Math.Atan2(2.0f * (ei - jk), 1 - 2.0f * (ii + jj));
            result.y = (float) Math.Asin(2.0f * (ej + ik));
            result.z = (float) Math.Atan2(2.0f * (ek - ij), 1 - 2.0f * (jj + kk));
            return result;
        }

        public static Vector3 QuaternionToXzy(Quaternion quaternion)
        {
            var ii = quaternion.x * quaternion.x;
            var jj = quaternion.y * quaternion.y;
            var kk = quaternion.z * quaternion.z;
            var ei = quaternion.w * quaternion.x;
            var ej = quaternion.w * quaternion.y;
            var ek = quaternion.w * quaternion.z;
            var ij = quaternion.x * quaternion.y;
            var ik = quaternion.x * quaternion.z;
            var jk = quaternion.y * quaternion.z;
            Vector3 result;
            result.x = (float) Math.Atan2(2.0f * (ei + jk), 1 - 2.0f * (ii + kk));
            result.y = (float) Math.Atan2(2.0f * (ej + ik), 1 - 2.0f * (jj + kk));
            result.z = (float) Math.Asin(2.0f * (ek - ij));
            return result;
        }

        public static Vector3 QuaternionToYxz(Quaternion quaternion)
        {
            var ii = quaternion.x * quaternion.x;
            var jj = quaternion.y * quaternion.y;
            var kk = quaternion.z * quaternion.z;
            var ei = quaternion.w * quaternion.x;
            var ej = quaternion.w * quaternion.y;
            var ek = quaternion.w * quaternion.z;
            var ij = quaternion.x * quaternion.y;
            var ik = quaternion.x * quaternion.z;
            var jk = quaternion.y * quaternion.z;
            Vector3 result;
            result.x = (float) Math.Asin(2.0f * (ei - jk));
            result.y = (float) Math.Atan2(2.0f * (ej + ik), 1 - 2.0f * (ii + jj));
            result.z = (float) Math.Atan2(2.0f * (ek + ij), 1 - 2.0f * (ii + kk));
            return result;
        }

        public static Vector3 QuaternionToYzx(Quaternion quaternion)
        {
            var ii = quaternion.x * quaternion.x;
            var jj = quaternion.y * quaternion.y;
            var kk = quaternion.z * quaternion.z;
            var ei = quaternion.w * quaternion.x;
            var ej = quaternion.w * quaternion.y;
            var ek = quaternion.w * quaternion.z;
            var ij = quaternion.x * quaternion.y;
            var ik = quaternion.x * quaternion.z;
            var jk = quaternion.y * quaternion.z;
            Vector3 result;
            result.x = (float) Math.Atan2(2.0f * (ei - jk), 1 - 2.0f * (ii + kk));
            result.y = (float) Math.Atan2(2.0f * (ej - ik), 1 - 2.0f * (jj + kk));
            result.z = (float) Math.Asin(2.0f * (ek + ij));
            return result;
        }

        public static Vector3 QuaternionToZxy(Quaternion quaternion)
        {
            var ii = quaternion.x * quaternion.x;
            var jj = quaternion.y * quaternion.y;
            var kk = quaternion.z * quaternion.z;
            var ei = quaternion.w * quaternion.x;
            var ej = quaternion.w * quaternion.y;
            var ek = quaternion.w * quaternion.z;
            var ij = quaternion.x * quaternion.y;
            var ik = quaternion.x * quaternion.z;
            var jk = quaternion.y * quaternion.z;
            Vector3 result;
            result.x = (float) Math.Asin(2.0f * (ei + jk));
            result.y = (float) Math.Atan2(2.0f * (ej - ik), 1 - 2.0f * (ii + jj));
            result.z = (float) Math.Atan2(2.0f * (ek - ij), 1 - 2.0f * (ii + kk));
            return result;
        }

        public static Vector3 QuaternionToZyx(Quaternion quaternion)
        {
            var ii = quaternion.x * quaternion.x;
            var jj = quaternion.y * quaternion.y;
            var kk = quaternion.z * quaternion.z;
            var ei = quaternion.w * quaternion.x;
            var ej = quaternion.w * quaternion.y;
            var ek = quaternion.w * quaternion.z;
            var ij = quaternion.x * quaternion.y;
            var ik = quaternion.x * quaternion.z;
            var jk = quaternion.y * quaternion.z;
            Vector3 result;
            result.x = (float) Math.Atan2(2.0f * (ei + jk), 1 - 2.0f * (ii + jj));
            result.y = (float) Math.Asin(2.0f * (ej - ik));
            result.z = (float) Math.Atan2(2.0f * (ek + ij), 1 - 2.0f * (jj + kk));
            return result;
        }
        
        public static Quaternion XyzToQuaternion(Vector3 euler) {
            var cx = Math.Cos(euler.x*0.5f);
            var sx = Math.Sin(euler.x*0.5f);
            var cy = Math.Cos(euler.y*0.5f);
            var sy = Math.Sin(euler.y*0.5f);
            var cz = Math.Cos(euler.z*0.5f);
            var sz = Math.Sin(euler.z*0.5f);
            Quaternion result;
            result.w = (float) (cx*cy*cz-sx*sy*sz);
            result.x = (float) (sx*cy*cz+cx*sy*sz);
            result.y = (float) (cx*sy*cz-sx*cy*sz);
            result.z = (float) (sx*sy*cz+cx*cy*sz);
            return result;
        }
        public static Quaternion XzyToQuaternion(Vector3 euler) {
            var cx = Math.Cos(euler.x*0.5f);
            var sx = Math.Sin(euler.x*0.5f);
            var cy = Math.Cos(euler.y*0.5f);
            var sy = Math.Sin(euler.y*0.5f);
            var cz = Math.Cos(euler.z*0.5f);
            var sz = Math.Sin(euler.z*0.5f);
            Quaternion result;
            result.w = (float) (cx*cy*cz+sx*sy*sz);
            result.x = (float) (sx*cy*cz-cx*sy*sz);
            result.y = (float) (cx*sy*cz-sx*cy*sz);
            result.z = (float) (cx*cy*sz+sx*sy*cz);
            return result;
        }
        public static Quaternion YxzToQuaternion(Vector3 euler) {
            var cx = Math.Cos(euler.x*0.5f);
            var sx = Math.Sin(euler.x*0.5f);
            var cy = Math.Cos(euler.y*0.5f);
            var sy = Math.Sin(euler.y*0.5f);
            var cz = Math.Cos(euler.z*0.5f);
            var sz = Math.Sin(euler.z*0.5f);
            Quaternion result;
            result.w = (float) (cx*cy*cz+sx*sy*sz);
            result.x = (float) (sx*cy*cz+cx*sy*sz);
            result.y = (float) (cx*sy*cz-sx*cy*sz);
            result.z = (float) (cx*cy*sz-sx*sy*cz);
            return result;
        }
        public static Quaternion YzxToQuaternion(Vector3 euler) {
            var cx = Math.Cos(euler.x*0.5f);
            var sx = Math.Sin(euler.x*0.5f);
            var cy = Math.Cos(euler.y*0.5f);
            var sy = Math.Sin(euler.y*0.5f);
            var cz = Math.Cos(euler.z*0.5f);
            var sz = Math.Sin(euler.z*0.5f);
            Quaternion result;
            result.w = (float) (cx*cy*cz-sx*sy*sz);
            result.x = (float) (sx*cy*cz+cx*sy*sz);
            result.y = (float) (cx*sy*cz+sx*cy*sz);
            result.z = (float) (cx*cy*sz-sx*sy*cz);
            return result;
        }
        public static Quaternion ZxyToQuaternion(Vector3 euler) {
            var cx = Math.Cos(euler.x*0.5f);
            var sx = Math.Sin(euler.x*0.5f);
            var cy = Math.Cos(euler.y*0.5f);
            var sy = Math.Sin(euler.y*0.5f);
            var cz = Math.Cos(euler.z*0.5f);
            var sz = Math.Sin(euler.z*0.5f);
            Quaternion result;
            result.w = (float) (cx*cy*cz-sx*sy*sz);
            result.x = (float) (sx*cy*cz-cx*sy*sz);
            result.y = (float) (cx*sy*cz+sx*cy*sz);
            result.z = (float) (cx*cy*sz+sx*sy*cz);
            return result;
        }
        public static Quaternion ZYXToQuaternion(Vector3 euler) {
            var cx = Math.Cos(euler.x*0.5f);
            var sx = Math.Sin(euler.x*0.5f);
            var cy = Math.Cos(euler.y*0.5f);
            var sy = Math.Sin(euler.y*0.5f);
            var cz = Math.Cos(euler.z*0.5f);
            var sz = Math.Sin(euler.z*0.5f);
            Quaternion result;
            result.w = (float) (cx*cy*cz+sx*sy*sz);
            result.x = (float) (sx*cy*cz-cx*sy*sz);
            result.y = (float) (cx*sy*cz+sx*cy*sz);
            result.z = (float) (cx*cy*sz-sx*sy*cz);
            return result;
        }

        public static float NanToZero(float f)
        {
            return float.IsNaN(f) ? 0.0f : f;
        }
    }
}