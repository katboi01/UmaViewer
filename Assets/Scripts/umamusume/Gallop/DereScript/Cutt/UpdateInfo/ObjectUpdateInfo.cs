using UnityEngine;

namespace Cutt
{
    public struct ObjectUpdateInfo
    {
        public float progressTime;

        public LiveTimelineObjectData data;

        public LiveTimelineTransformData.TransformBaseData updateData;

        public bool renderEnable;

        public bool reflectionEnable;

        public bool colorEnable;

        public Color color;
    }
}
