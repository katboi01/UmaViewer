using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyPostFilmData : LiveTimelineKeyWithInterpolate
    {
        public enum eLayerMode
        {
            Color,
            UVMovie
        }

        public enum eColorBlend
        {
            None,
            Lerp,
            Additive,
            Multiply
        }

        public PostFilmMode filmMode;

        public PostColorType colorType;

        public float filmPower;

        public Vector2 filmOffsetParam = Vector2.zero;

        public Vector4 filmOptionParam = Vector4.zero;

        public Color color0 = Color.black;

        public Color color1 = Color.black;

        public Color color2 = Color.black;

        public Color color3 = Color.black;

        public bool isScreenCircle;

        public Vector2 screenCircleDir = Vector2.one;

        private const int kAttrUseAlphaMask = 0x40000;

        private const int kAttrReverseUVMovie = 0x80000;

        private const int kAttrInverseVignette = 0x100000;

        public eLayerMode layerMode;

        public int movieResId;

        public int movieFrameOffset;

        public float movieSpeed = 1f;

        public eColorBlend colorBlend;

        public float colorBlendFactor;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.PostFilm;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }

        public bool isUseTexMask()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrUseAlphaMask);
        }

        public bool isReverseUVMovie()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrReverseUVMovie);
        }

        public bool isInverseVignette()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrInverseVignette);
        }
    }
}

