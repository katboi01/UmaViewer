using System;
using UnityEngine;

[Serializable]
public class A2UMultiSprite : ScriptableObject
{
    [Serializable]
    public class SpriteInfo
    {
        public Rect rect;

        public Vector2 pivot;

        public Vector4 border;

        public float pixelsToUnits;

        public uint extrude;

        public SpriteMeshType meshType;

        public string name;
    }

    public SpriteInfo[] _spriteInfos;
}
