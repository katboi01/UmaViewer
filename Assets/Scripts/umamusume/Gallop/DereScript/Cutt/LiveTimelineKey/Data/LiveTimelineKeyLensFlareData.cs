using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyLensFlareData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 offset = Vector3.zero;

        public Color color = Color.white;

        public float brightness = 1f;

        public float fadeSpeed = 1f;
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.LensFlare;
    }
}
