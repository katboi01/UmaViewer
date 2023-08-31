using UnityEngine;

namespace LibMMD.Util
{
    public static class TransformExtensions
    {
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.localScale = matrix.ExtractScale();
            transform.rotation = matrix.ExtractRotation();
            transform.position = matrix.ExtractPosition();
        }
    }
}