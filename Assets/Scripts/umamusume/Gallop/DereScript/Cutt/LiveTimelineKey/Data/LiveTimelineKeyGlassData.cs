using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyGlassData : LiveTimelineKeyWithInterpolate
    {
        public float transparent;

        public float specularPower = 5f;

        public Color specularColor = Color.white;

        public Vector3 lightPosition = Vector3.zero;

        public int sortOrderOffset;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Glass;
    }
}
