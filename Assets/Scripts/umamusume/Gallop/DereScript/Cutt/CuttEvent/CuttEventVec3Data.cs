using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public struct CuttEventVec3Data
    {
        public float x;

        public float y;

        public float z;

        public static implicit operator CuttEventVec3Data(Vector3 data)
        {
            CuttEventVec3Data result = default(CuttEventVec3Data);
            result.x = data.x;
            result.y = data.y;
            result.z = data.z;
            return result;
        }

        public static explicit operator Vector3(CuttEventVec3Data data)
        {
            return new Vector3(data.x, data.y, data.z);
        }
    }
}
