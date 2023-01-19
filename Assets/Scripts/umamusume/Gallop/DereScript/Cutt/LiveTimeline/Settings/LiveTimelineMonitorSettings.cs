using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineMonitorSettings
    {
        public int charaTextureNum;

        public LiveCharaPosition[] charaTexturePosition = new LiveCharaPosition[0];
    }
}
