using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyObjectData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 position = Vector3.zero;

        public Vector3 rotate = Vector3.zero;

        public Vector3 scale = Vector3.one;

        public bool renderEnable = true;

        public bool reflectionEnable = true;

        public bool colorEnable;

        public Color color = Color.white;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Object;
    }
}
