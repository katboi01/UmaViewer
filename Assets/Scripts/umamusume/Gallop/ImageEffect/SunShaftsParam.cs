using UnityEngine;

namespace Gallop.ImageEffect
{
    public class SunShaftsParam : MonoBehaviour
    {
        public enum SunShaftsResolution
        {
            Low = 0,
            Normal = 1,
            High = 2
        }

        public enum SunShaftsScreenBlendMode
        {
            Screen = 0,
            Add = 1
        }

        public bool IsEnable; // 0x10
        public Camera TargetCamera; // 0x18
        public Transform SunTransform; // 0x20
        public Vector3 SunPosition; // 0x28
        public SunShaftsParam.SunShaftsResolution Resolution; // 0x34
        public SunShaftsParam.SunShaftsScreenBlendMode ScreenBlendMode; // 0x38
        public float SunShaftBlurRadius; // 0x3C
        public Color SunColor; // 0x40
        public float SunPower; // 0x50
        public float CenterBrightness; // 0x54
        public float CenterMultiplex; // 0x58
        public float KomorebiRate; // 0x5C
        public bool IsEnabledBorderClear; // 0x60
        public float SunShaftIntensity; // 0x64
        public float BlackLevel; // 0x68
        public int BlurIterations; // 0x6C
    }
}

