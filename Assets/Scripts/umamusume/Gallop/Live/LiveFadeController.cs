using UnityEngine;

namespace Gallop.Live
{
    public class LiveFadeController
    {
        //private RawImageCommon _landscapeImage; // 0x10
        //private RawImageCommon _portraitImage; // 0x18
        private float _startCurrentTime; // 0x20
        private float _duration; // 0x24
        private Color _color; // 0x28
        private Color _startColor; // 0x38
        private Color _targetColor; // 0x48
        private bool _isInitialized; // 0x58
    }
}
