using UnityEngine;

namespace Cutt
{
    public struct TextureAnimationUpdateInfo
    {
        public float progressTime;

        public LiveTimelineTextureAnimationData data;

        public string textureName;

        public bool textureNameEmpty;

        public Vector2 offset;

        public Vector2 tiling;

        public Vector2 scrollSpeed;

        public float scrollInterval;

        public int textureID;
    }
}
