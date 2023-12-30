namespace Gallop.Live
{
    public class LiveTimeController
    {
        private readonly float SmoothingTargetDeltaTime; // 0x10
        private readonly float SmoothingDeltaTimeThreshold; // 0x14
        private readonly float SmoothingOvertimeClamp; // 0x18
        private bool _isEnabledSmooth; // 0x20
        private float _smoothingWorkTime; // 0x28
        private float _lastMusicTimeWithoutDelay; // 0x2C
        private float _currentTime; // 0x30
        private float _prevTime; // 0x34
        private float _deltaTime; // 0x38
        private bool _doCalcElapsedTime; // 0x3C
        private float _elapsedTime; // 0x40
        private bool _isPause; // 0x44

        public float MusicTime { get; set; }
        public float SmoothMusicTime { get; set; }
        public float CurrentTime { get; set; }
        public float DeltaTime { get; }
        public bool IsCalcElapsedTime { get; }
        public bool IsPause { get; }
    }
}
