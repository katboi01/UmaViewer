namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyParticleData : LiveTimelineKeyWithInterpolate
    {
        public float emissionRate;
    }

    [System.Serializable]
    public class LiveTimelineKeyParticleDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyParticleData>
    {
    }


    [System.Serializable]
    public class LiveTimelineParticleData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyParticleDataList keys;
    }
}
