using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineHdrBloomSettings
    {
        public float sepBlurSpread = 2.5f;

        public float bloomIntensity = 0.3f;

        public int bloomBlurIterations = 1;
    }
}
