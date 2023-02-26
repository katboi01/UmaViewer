using UnityEngine;

namespace Cutt
{
    public static class CuttVector3_Helper
    {
        public static Vector3 Round(this Vector3 This)
        {
            float num = 1000f;
            This.x = Mathf.Round(This.x * num) / num;
            This.y = Mathf.Round(This.y * num) / num;
            This.z = Mathf.Round(This.z * num) / num;
            return This;
        }
    }
}
