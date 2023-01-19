using UnityEngine;

namespace Cutt
{
    public struct GlobalFogUpdateInfo
    {
        public bool isDistance;

        public float startDistance;

        public bool isHeight;

        public float height;

        public float heightDensity;

        public Color color;

        public FogMode fogMode;

        public float expDensity;

        public float start;

        public float end;

        public bool useRadialDistance;

        public Vector4 heightOption;

        public Vector4 distanceOption;
    }
}