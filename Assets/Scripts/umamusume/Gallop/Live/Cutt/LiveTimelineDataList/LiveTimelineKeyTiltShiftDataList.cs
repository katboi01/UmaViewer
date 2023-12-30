using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyTiltShiftData : LiveTimelineKeyWithInterpolate
    {
        public enum Mode
        {
            None = 0,
            TiltShiftMode = 1,
            IrisMode = 2
        }

        public enum Quality
        {
            Preview = 0,
            Normal = 1,
            High = 2
        }


        public LiveTimelineKeyTiltShiftData.Mode mode;
        public LiveTimelineKeyTiltShiftData.Quality quality;
        public float blurArea;
        public float maxBlurSize;
        public int downsample;
        public Vector2 offset;
        public float roll;
    }

    [System.Serializable]
    public class LiveTimelineKeyTiltShiftDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyTiltShiftData>
    {

    }
}
