using UnityEngine;

namespace Gallop.Live
{
    public class HandShakeCamera
    {
        public struct Work
        {
            public float Power; // 0x0
            public float Frequency; // 0x4
            public Vector3 CameraRightVector; // 0x8
            public float DeltaTime; // 0x14
            private float _currentAngle; // 0x18
            private Vector2 _screenPosition; // 0x1C
            private Vector2 _nextTarget; // 0x24
            private float _duration; // 0x2C
        }

        private const float TIME_RANGE = 0.1f;
        private const float DELTA_TIME_FACTOR = 30;
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _power; // 0x10
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _frequency; // 0x14
        private Vector3 _position; // 0x18
        private HandShakeCamera.Work _work; // 0x24
    }
}
