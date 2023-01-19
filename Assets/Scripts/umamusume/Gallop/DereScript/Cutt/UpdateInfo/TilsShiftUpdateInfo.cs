using UnityEngine;

namespace Cutt
{
    public struct TilsShiftUpdateInfo
    {
        public LiveTimelineKeyTiltShiftData.Mode mode;

        public LiveTimelineKeyTiltShiftData.Quality quality;

        public float blurArea;

        public float maxBlurSize;

        public int downsample;

        public Vector2 offset;

        public float roll;

        public Vector2 blurDir;

        public Vector2 blurAreaDir;
    }
}
