using System;

namespace Cutt
{
    [Serializable]
    public class CuttEventParam_CameraHandShake : CuttEventParamBase
    {
        public bool isEnable = true;

        public bool isOverwrite;

        public float positionAmount = LiveTimelineCamera.kPositionAmountInitVal;

        public float rotationAmount = LiveTimelineCamera.kRotationAmountInitVal;

        public CuttEventVec3Data positionComponents = LiveTimelineCamera.kPositionComponentsInitVal;

        public CuttEventVec3Data rotationComponents = LiveTimelineCamera.kRotationComponentsInitVal;
    }
}
