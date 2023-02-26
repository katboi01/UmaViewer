using System;

namespace Cutt
{
    [Serializable]
    public class CuttEventParam_ChangeEyeTrackTarget : CuttEventParamBase
    {
        public enum Target
        {
            Arena,
            Camera,
            StageLeftSide,
            StageRightSide
        }

        public Target target;
    }
}
