using UnityEngine;

namespace Gallop.Live
{
    public class LiveFaceController : MonoBehaviour
    {
        private class FacePartsSetInfo
        {
            public FacePartsSet[] FacePartsSetArray; // 0x10
            public int EyeRCount; // 0x18
            public int EyeLCount; // 0x1C
            public int EyebrowRCount; // 0x20
            public int EyebrowLCount; // 0x24
            public int MouthCount; // 0x28
        }

        private enum BlinkStep
        {
            WaitStart = 0,
            SyncBlink = 1
        }

        private enum FacialKeyType
        {
            None = 0,
            Face = 1,
            Eye = 2
        }

        private const bool ENABLE_AUTO_BLINK = false;
        private const float AUTO_BLINK_REPRESSION_TIME = 1;
        private const int FACE_PARTS_SET_TEMP_ITEM_NUM = 1;
        private float _earWeight; // 0x18
        private float _earDurationTime; // 0x1C
        private float _eyebrowDurationTime; // 0x20
        private float _eyeDurationTime; // 0x24
        private float _mouthDurationTime; // 0x28
        private DrivenKeyComponent.InterpolateType _eyeInterpolateType; // 0x2C
        private DrivenKeyComponent.InterpolateType _eyebrowInterpolateType; // 0x30
        private DrivenKeyComponent.InterpolateType _mouthInterpolateType; // 0x34
        private DrivenKeyComponent.InterpolateType _earInterpolateType; // 0x38
        private DrivenKeyComponent.InterpolateType _prevEyeInterpolateType; // 0x3C
        private DrivenKeyComponent.InterpolateType _prevEyebrowInterpolateType; // 0x40
        private DrivenKeyComponent.InterpolateType _prevMouthInterpolateType; // 0x44
        private DrivenKeyComponent.InterpolateType _prevEarInterpolateType; // 0x48
        private EarType _earTypeL; // 0x4C
        private EarType _earTypeR; // 0x50
        private LiveFaceController.FacePartsSetInfo _facePartsSet; // 0x58
        private FacePartsSet _facePartsSetTemp; // 0x60
        private LiveFaceController.FacePartsSetInfo _prevFacePartsSetInfo; // 0x68
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private LiveFaceController.BlinkStep _autoBlinkStep; // 0x70
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _autoBlinkTimeCount; // 0x74
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _autoBlinkTimeInterval; // 0x78
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private int _cheekType; // 0x7C
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private int _tearyType; // 0x80
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private int _tearfulType; // 0x84
        private ModelController _modelController; // 0x88
        private float _prevEarWeight; // 0x90
        private float _prevEarDurationTime; // 0x94
        private float _prevEyebrowDurationTime; // 0x98
        private float _prevEyeDurationTime; // 0x9C
        private float _prevMouthDurationTime; // 0xA0
        private EarType _prevEarTypeL; // 0xA4
        private EarType _prevEarTypeR; // 0xA8
        private bool _isRepressAutoBlink; // 0xAC
        private bool _isLipSync; // 0xAD
        private const float AutoBlinkIntervalMin = 3;
        private const float AutoBlinkIntervalMax = 5;
        private int _blinkRno; // 0xB0
        private int _currentStartFrameOfEye; // 0xB4

        public bool IsUpdateAutoBlink { get; set; }
    }
}
