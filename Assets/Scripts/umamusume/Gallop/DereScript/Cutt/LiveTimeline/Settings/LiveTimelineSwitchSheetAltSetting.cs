using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineSwitchSheetAltSetting
    {
        [Serializable]
        public class LeftHanded
        {
            public int[] targetCharaList = new int[0];

            public int targetCharaCount;
        }

        public LiveTimelineData.AlterSheetMode alterSheetMode;

        public LeftHanded leftHanded;

        public void Init()
        {
            if (alterSheetMode == LiveTimelineData.AlterSheetMode.LeftHanded)
            {
                if (leftHanded == null)
                {
                    leftHanded = new LeftHanded();
                }
            }
            else
            {
                leftHanded = null;
            }
        }
    }
}
