using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyBgColor2Data : LiveTimelineKeyWithInterpolate
    {
        public Color color1 = Color.white;

        public Color color2 = Color.white;

        public float f32;

        public bool manualAngle;

        public Vector3 angle = Vector3.zero;

        [NonSerialized]
        public Quaternion manualRotation = Quaternion.identity;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.BgColor2;

        public int RandomTableIndex()
        {
            return attribute.GetHighWord();
        }
    }
}
