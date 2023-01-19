using UnityEngine;

namespace Cutt
{
    public struct GlassUpdateInfo
    {
        public float transparent;

        public float specularPower;

        public Color specularColor;

        public Vector3 lightPosition;

        public int sortOrderOffset;

        public LiveTimelineGlassData data;
    }
}