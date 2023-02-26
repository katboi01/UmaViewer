using UnityEngine;

namespace Cutt
{
    public struct CharaIKUpdateInfo
    {
        public struct PartUpdateInfo
        {
            public StageTwoBoneIK.TargetType targetPositionType;

            public Vector3 targetPosition;

            public float blendRate;
        }

        public LiveCharaPosition position;

        public PartUpdateInfo leg_Left;

        public PartUpdateInfo leg_Right;

        public PartUpdateInfo arm_Right;

        public PartUpdateInfo arm_Left;
    }
}
