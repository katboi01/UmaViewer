using UnityEngine;

namespace Cutt
{
    public struct PostEffectUpdateInfo
    {
        public float forcalSize;

        public float blurSpread;

        public float bloomDofWeight;

        public float threshold;

        public float intensity;

        public Vector3 forcalPosition;

        public PostEffectLive3D.DofQuality dofQuality;

        public PostEffectLive3D.DofBlurType dofBlurType;

        public float dofForegroundSize;

        public float dofFocalPoint;

        public bool isUseFocalPoint;

        public PostEffectLive3D.eDofMVFilterType dofMVFilterType;

        public int filterResId;

        public float filterIntensity;

        public float filterTime;

        public bool disableDOFBlur;
    }
}
