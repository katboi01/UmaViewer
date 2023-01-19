using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyTextureAnimationData : LiveTimelineKeyWithInterpolate
    {
        public string textureName = "";

        public bool textureNameEmpty = true;

        public Vector2 offset;

        public Vector2 tiling = Vector2.one;

        public Vector2 scrollSpeed;

        public float scrollInterval = 1f;

        public int textureID = -1;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.TextureAnimation;
    }
}
