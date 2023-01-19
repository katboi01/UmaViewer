using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public struct CuttEventVec2Data
    {
        public float x;

        public float y;

        public static implicit operator CuttEventVec2Data(Vector2 data)
        {
            CuttEventVec2Data result = default(CuttEventVec2Data);
            result.x = data.x;
            result.y = data.y;
            return result;
        }

        public static explicit operator Vector2(CuttEventVec2Data data)
        {
            return new Vector2(data.x, data.y);
        }
    }
}
