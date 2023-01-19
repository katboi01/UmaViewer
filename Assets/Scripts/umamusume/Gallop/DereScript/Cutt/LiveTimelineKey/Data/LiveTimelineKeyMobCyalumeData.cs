using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalumeData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 translate = Vector3.zero;

        public Vector3 rotate = Vector3.zero;

        public Vector3 scale = Vector3.one;

        public bool isMobVisible = true;

        public bool isCyalumeVisible = true;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume;
    }
}

