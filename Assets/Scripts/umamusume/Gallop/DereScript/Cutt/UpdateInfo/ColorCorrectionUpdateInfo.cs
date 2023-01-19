using UnityEngine;

namespace Cutt
{
    public struct ColorCorrectionUpdateInfo
    {
        public bool _enable;

        public float _saturation;

        public AnimationCurve _redCurve;

        public AnimationCurve _greenCurve;

        public AnimationCurve _blueCurve;

        public bool _selective;

        public Color _keyColor;

        public Color _targetColor;

        public int _cameraIndex;
    }
}