using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineSunshaftsSettings
    {
        public PostEffectLive3D.SunShaftsResolution resolution = PostEffectLive3D.SunShaftsResolution.Normal;

        public PostEffectLive3D.ShaftsScreenBlendMode screenBlendMode = PostEffectLive3D.ShaftsScreenBlendMode.Add;

        public Color sunColor = Color.white;

        public float sunPower;

        public float intensity = 0.5f;

        public float fadeStart = 40f;

        public float fadeMix = 18f;

        public float blackLevel = 0.1f;

        public float komorebiRate;

        public float blurRadius = 5f;

        public int blurIterations = 2;

        public bool isEnabledBorderClear;
    }
}
