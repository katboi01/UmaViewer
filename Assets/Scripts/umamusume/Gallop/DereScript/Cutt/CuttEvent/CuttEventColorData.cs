using UnityEngine;

namespace Cutt
{
    public struct CuttEventColorData
    {
        public float a;

        public float r;

        public float g;

        public float b;

        public static implicit operator CuttEventColorData(Color data)
        {
            CuttEventColorData result = default(CuttEventColorData);
            result.a = data.a;
            result.r = data.r;
            result.g = data.g;
            result.b = data.b;
            return result;
        }

        public static explicit operator Color(CuttEventColorData data)
        {
            return new Color(data.r, data.g, data.b, data.a);
        }
    }
}
