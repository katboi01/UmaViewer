using UnityEngine;

namespace Cutt
{
    public struct A2UUpdateInfo
    {
        public float progressTime;

        public LiveTimelineA2UData data;

        public Color spriteColor;

        public Vector2 position;

        public Vector2 scale;

        public float rotationZ;

        public uint textureIndex;

        public int appearanceRandomSeed;

        public float spriteAppearance;

        public int slopeRandomSeed;

        public float spriteMinSlope;

        public float spriteMaxSlope;

        public float spriteScale;

        public float spriteOpacity;

        public float startSec;

        public float speed;

        public A2U.Blend blend;

        public bool isFlick;

        public bool enable;
    }
}