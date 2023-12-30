using Gallop.ImageEffect;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyPostEffectBloomDiffusionData : LiveTimelineKeyWithInterpolate
    {
        public float bloomDofWeight;
        public float threshold;
        public float intensity;
        public float BloomBlurSize;
        public DofDiffusionBloomOverlayParam.BloomScreenBlendMode BloomBlendMode;
        public float diffusionBlurSize;
        public float diffusionBright;
        public float diffusionThreshold;
        public float diffusionSaturation;
        public float diffusionContrast;

    }

    [System.Serializable]
    public class LiveTimelineKeyPostEffectBloomDiffusionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyPostEffectBloomDiffusionData>
    {

    }
}