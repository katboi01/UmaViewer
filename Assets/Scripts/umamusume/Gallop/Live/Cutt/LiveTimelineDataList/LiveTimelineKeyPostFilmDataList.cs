using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum PostFilmMode
    {
        None = 0,
        Lerp = 1,
        Add = 2,
        Mul = 3,
        VignetteLerp = 4,
        VignetteAdd = 5,
        VignetteMul = 6,
        Monochrome = 7
    }

    public enum PostColorType
    {
        ColorAll = 0,
        Color2TopBottom = 1,
        Color2LeftRight = 2,
        Color4 = 3
    }


    [System.Serializable]
    public class LiveTimelineKeyPostFilmData : LiveTimelineKeyWithInterpolate
    {
        public enum LayerMode
        {
            Color = 0,
            UVMovie = 1
        }

        public enum ColorBlend
        {
            None = 0,
            Lerp = 1,
            Additive = 2,
            Multiply = 3
        }

        public PostFilmMode filmMode; // 0x30
        public PostColorType colorType; // 0x34
        public float filmPower; // 0x38
        public Vector2 filmOffsetParam; // 0x3C
        public Vector4 filmOptionParam; // 0x44
        public Color color0; // 0x54
        public Color color1; // 0x64
        public Color color2; // 0x74
        public Color color3; // 0x84
        public float depthPower; // 0x94
        public float DepthClip; // 0x98
        public float RollAngle; // 0x9C
        public Vector2 FilmScale; // 0xA0
        public LiveTimelineKeyLoopType loopType; // 0xA8
        public int loopCount; // 0xAC
        public int loopExecutedCount; // 0xB0
        public int loopIntervalFrame; // 0xB4
        public bool isPasteLoopUnit; // 0xB8
        public bool isChangeLoopInterpolate; // 0xB9
        public LiveTimelineKeyPostFilmData.LayerMode layerMode; // 0xBC
        public int movieResId; // 0xC0
        public int movieFrameOffset; // 0xC4
        public float movieSpeed; // 0xC8
        public LiveTimelineKeyPostFilmData.ColorBlend colorBlend; // 0xCC
        public float colorBlendFactor; // 0xD0
    }
}
