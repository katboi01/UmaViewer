using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyA2UData : LiveTimelineKeyWithInterpolate
    {
        public Color spriteColor = Color.white;

        public Vector2 position = new Vector2(0f, 0f);

        public Vector2 scale = new Vector2(1f, 1f);

        public float rotationZ;

        public int textureIndex;

        public int appearanceRandomSeed = 123456789;

        public float spriteAppearance = 1f;

        public int slopeRandomSeed = 123456789;

        public float spriteMinSlope;

        public float spriteMaxSlope;

        public float spriteScale = 1f;

        public float spriteOpacity = 1f;

        public float startSec;

        public float speed = 1f;

        public bool isFlick = true;

        public bool enable = true;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.A2U;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}
