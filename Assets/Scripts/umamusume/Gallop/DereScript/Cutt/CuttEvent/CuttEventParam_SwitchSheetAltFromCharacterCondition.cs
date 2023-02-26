using System;
using Cutt;

namespace Cutt
{
    [Serializable]
    public class CuttEventParam_SwitchSheetAltFromCharacterCondition : CuttEventParamBase
    {
        public bool isSheetAlt;

        public LiveTimelineData.AlterSheetMode alterSheetMode;

        public LiveTimelineSwitchSheetAltSetting.LeftHanded leftHanded;
    }
}
