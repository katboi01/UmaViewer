using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMonitorCameraSettings
    {
        public int cntCamera;

        public float renderTextureWidth;

        public float renderTextureHeight;

        [NonSerialized]
        public static float MINIMUM_WIDTH = 128f;

        [NonSerialized]
        public static float MAXIMUM_WIDTH = 1280f;

        [NonSerialized]
        public static float MINIMUM_HEIGHT = 128f;

        [NonSerialized]
        public static float MAXIMUM_HEIGHT = 1280f;
    }
}
