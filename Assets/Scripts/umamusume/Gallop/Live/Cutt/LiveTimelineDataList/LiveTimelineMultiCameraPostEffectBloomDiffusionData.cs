namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPostEffectBloomDiffusionData : LiveTimelineKeyPostEffectBloomDiffusionData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPostEffectBloomDiffusionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMultiCameraPostEffectBloomDiffusionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMultiCameraPostEffectBloomDiffusionData : LiveTimelineMultiCameraPostEffect
    {
        private const string default_name = "Bloom/Diffusion";
        public LiveTimelineKeyMultiCameraPostEffectBloomDiffusionDataList keys;
    }
}
