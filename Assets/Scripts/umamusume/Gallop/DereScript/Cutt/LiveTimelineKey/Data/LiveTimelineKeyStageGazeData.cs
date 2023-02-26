using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyStageGazeData : LiveTimelineKeyWithInterpolate
    {
        public bool isEnable;

        public Vector3 targetPosition;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.StageGaze;
    }
}
