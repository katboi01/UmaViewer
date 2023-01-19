using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyLightShuftData : LiveTimelineKeyWithInterpolate
    {
        public bool enabled = true;

        public Vector4 speed = new Vector4(-60f, -30f, -120f, -30f);

        public Vector4 angle = new Vector4(0f, 0f, 30f, 0f);

        public Vector4 offset;

        public Vector4 alpha = new Vector4(1f, 1f, 1f, 1f);

        public Vector4 alpha2 = new Vector4(1f, 1f, 1f, 1f);

        public Vector4 maskAlpha = new Vector4(1f, 1f, 1f, 1f);

        public float maskAnimeTime = 0.75f;

        public Vector2 maskAlphaRange = new Vector2(0f, 1f);

        public float scale = 1f;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.LightShuft;
    }
}
