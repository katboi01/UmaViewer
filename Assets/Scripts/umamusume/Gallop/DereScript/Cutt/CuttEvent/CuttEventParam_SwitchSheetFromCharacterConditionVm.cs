using System;

namespace Cutt
{
    [Serializable]
    public class CuttEventParam_SwitchSheetFromCharacterConditionVm : CuttEventParamBase
    {
        public bool isSheetAlt;

        public int conditionIndex;

        public int sheetIndex;
    }
}

