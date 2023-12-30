using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyWaveObjectData : LiveTimelineKeyWithInterpolate
    {
        public bool IsWorldDir;
        public Vector3 WaveDir;
        public float WaveFreq;
        public float WaveSpeed;
        public float WaveSize;
    }

    [System.Serializable]
    public class LiveTimelineKeyWaveObjectDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyWaveObjectData>
    {

    }

    [System.Serializable]
    public class LiveTimelineWaveObjectData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyWaveObjectDataList keys;
    }
}
