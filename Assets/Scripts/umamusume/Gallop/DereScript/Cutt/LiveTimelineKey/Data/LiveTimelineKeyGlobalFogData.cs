using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyGlobalFogData : LiveTimelineKeyWithInterpolate
    {
        public bool isDistance;

        public float startDistance;

        public bool isHeight;

        public float height = 2f;

        public float heightDensity = 1f;

        public Color color;

        public FogMode fogMode = FogMode.ExponentialSquared;

        public float expDensity = 0.01f;

        public float start;

        public float end = 300f;

        public bool useRadialDistance;

        public Vector4 heightOption = new Vector4(1f, 0f, 0f, 0f);

        public Vector4 distanceOption = new Vector4(1f, 0f, 1f, 0f);

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.GlobalFog;
    }
}
