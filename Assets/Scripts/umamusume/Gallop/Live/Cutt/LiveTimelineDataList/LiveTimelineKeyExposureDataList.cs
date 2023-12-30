namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyExposureData : LiveTimelineKeyWithInterpolate
    {
        public bool IsEnable;
        public float DepthMask;
        public float Gain;
        public float Lift;
        public float MaskGain;
        public float MaskLift;
    }

    [System.Serializable]
    public class LiveTimelineKeyExposureDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyExposureData>
    {

    }
}
