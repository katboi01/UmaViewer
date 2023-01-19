namespace Cutt
{
    public struct ShaderControlUpdateInfo
    {
        public LiveTimelineKeyShaderControlData.eCondition condition;

        public int conditionParam;

        public int targetFlags;

        public int behaviorFlags;

        public bool useVtxClrB;

        public float lerpDiffuse;

        public float lerpGradation;

        public LiveTimelineShaderControlData data;
    }
}