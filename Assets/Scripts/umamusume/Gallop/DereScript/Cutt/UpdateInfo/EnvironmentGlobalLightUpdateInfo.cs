using UnityEngine;

namespace Cutt
{
    public struct EnvironmentGlobalLightUpdateInfo
    {
        public LiveTimelineGlobalLightData data;

        public Vector3 lightDirection;

        public float globalRimRate;

        public float globalRimShadowRate;

        public float globalRimSpecularRate;

        public float globalToonRate;
    }
}