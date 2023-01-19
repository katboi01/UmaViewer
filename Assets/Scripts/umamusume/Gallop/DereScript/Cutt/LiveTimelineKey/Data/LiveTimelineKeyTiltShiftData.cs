using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyTiltShiftData : LiveTimelineKeyWithInterpolate
    {
        public enum Mode
        {
            None,
            TiltShiftMode,
            IrisMode,
            Circle
        }

        public enum Quality
        {
            Preview,
            Normal,
            High
        }

        public Mode mode = Mode.IrisMode;

        public Quality quality = Quality.High;

        public float blurArea = 1f;

        public float maxBlurSize = 5f;

        public int downsample;

        public Vector2 offset;

        public float roll;

        public Vector2 blurDir = Vector2.one;

        public Vector2 blurAreaDir = Vector2.one;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.TiltShift;
    }
}
