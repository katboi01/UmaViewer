using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineCharacterSettings
    {
        public int[] motionSequenceIndices = new int[5];

        public int[] motionOverwriteIndices = new int[15];

        public bool[] useHighPolygonModel = new bool[15] { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

        public bool[] useHighPolygonModelForLightMode = new bool[15] { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

        public bool isCreateGroundCollision = true;

        public float distanceForOutlineLOD = (float)Math.PI;

        public float distanceForShaderLOD = (float)Math.PI;

        public float distanceForCheekLOD = 1f;

        public bool isCharaAxisRotateLateUpdate;
    }
}
