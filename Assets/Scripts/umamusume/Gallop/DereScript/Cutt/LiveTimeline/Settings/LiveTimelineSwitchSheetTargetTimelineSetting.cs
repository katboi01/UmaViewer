using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineSwitchSheetTargetTimelineSetting
    {
        public enum Timeline
        {
            Camera,
            PostEffect,
            FormationOffset,
            PropsAttachControl,
            Facial,
            Max
        }

        public bool[] isTargetTimelineList = new bool[5];

        public void SetAllOff()
        {
            for (int i = 0; i < 5; i++)
            {
                isTargetTimelineList[i] = false;
            }
        }

        public void SetAllOn()
        {
            for (int i = 0; i < 5; i++)
            {
                isTargetTimelineList[i] = true;
            }
        }

        public void Set(Timeline timeline, bool flag)
        {
            if (Timeline.Camera <= timeline && (int)timeline < isTargetTimelineList.Length)
            {
                isTargetTimelineList[(int)timeline] = flag;
            }
        }

        public bool Get(Timeline timeline)
        {
            if (Timeline.Camera <= timeline && (int)timeline < isTargetTimelineList.Length)
            {
                return isTargetTimelineList[(int)timeline];
            }
            return false;
        }
    }
}
