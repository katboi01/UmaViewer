using UnityEngine;

namespace Cutt
{
    public struct PostFilmUpdateInfo
    {
        public PostFilmMode filmMode;

        public PostColorType colorType;

        public float filmPower;

        public Vector2 filmOffsetParam;

        public Vector4 filmOptionParam;

        public Color color0;

        public Color color1;

        public Color color2;

        public Color color3;

        public LiveTimelineKeyPostFilmData.eLayerMode layerMode;

        public LiveTimelineKeyPostFilmData.eColorBlend colorBlend;

        public bool inverseVignette;

        public float colorBlendFactor;

        public int movieResId;

        public int movieFrameOffset;

        public float movieTime;

        public bool movieReverse;

        public bool screenCircle;

        public Vector2 screenCircleDir;
    }
}
