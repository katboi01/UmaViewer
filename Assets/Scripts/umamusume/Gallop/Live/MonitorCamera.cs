using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class MonitorCamera : MonoBehaviour
    {
        public const int DEFAULT_DEPTH = 16;
        public const float TEXTURE_WIDTH_RATE_MIN = 0.25f;
        public const float TEXTURE_WIDTH_RATE_MAX = 1;
        public const float TEXTURE_WIDTH_RATE_DEF = 0.25f;
        private Camera _targetCamera; // 0x18
        private bool _isEnabledTargetTexture; // 0x20
        private RenderTexture _targetTexture; // 0x28
        private RenderTexture _monitorTexture; // 0x30

        public Camera TargetCamera { get; }
        public RenderTexture MonitorTexture { get; }
        public static float TextureWidthRate { get; set; }
    }
}
