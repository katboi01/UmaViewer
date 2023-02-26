using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyPostEffectData : LiveTimelineKeyWithInterpolate
    {
        public const int kAttrUseLookAt = 65536;

        public const int kAttrUseFocalPoint = 131072;

        public const int kAttrDisableDOFBlur = 262144;

        public float forcalSize = 10f;

        public float blurSpread = 1.75f;

        public float bloomDofWeight = 0.3f;

        public float threshold = 0.25f;

        public float intensity = 0.75f;

        public LiveCharaPositionFlag charactor = LiveCharaPositionFlag.Center;

        public PostEffectLive3D.DofBlurType dofBlurType;

        public PostEffectLive3D.DofQuality dofQuality = PostEffectLive3D.DofQuality.OnlyBackground;

        public float dofForegroundSize = 0.1f;

        public float dofFgBlurSpread = 1f;

        public float dofFocalPoint = 1f;

        public PostEffectLive3D.eDofMVFilterType dofMVFilterType;

        public float dofMVFilterSpd = 1f;

        public int dofMVFilterResId;

        public float dofMVFilterIntensity = 1f;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.PostEffect;

        public bool IsUseFocalPoint()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrUseFocalPoint);
        }

        public bool IsUseLookAt()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrUseLookAt);
        }

        public bool IsDisableDOFBlur()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrDisableDOFBlur);
        }

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}

