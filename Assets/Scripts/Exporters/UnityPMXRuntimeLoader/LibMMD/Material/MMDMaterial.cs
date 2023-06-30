using UnityEngine;

namespace LibMMD.Material
{
    public class MMDMaterial
    {
        public enum SubTextureTypeEnum : byte
        {
            MatSubTexOff = 0,
            MatSubTexSph = 1,
            MatSubTexSpa = 2,
            MatSubTexSub = 3
        }

        public string Name { get; set; }
        public string NameEn { get; set; }

        public Color DiffuseColor { get; set; }
        public Color SpecularColor { get; set; }
        public Color AmbientColor { get; set; }

        public float Shiness { get; set; }

        public bool DrawDoubleFace { get; set; }
        public bool DrawGroundShadow { get; set; }
        public bool CastSelfShadow { get; set; }
        public bool DrawSelfShadow { get; set; }
        public bool DrawEdge { get; set; }

        public Color EdgeColor { get; set; }
        public float EdgeSize { get; set; }

        public MMDTexture Toon { get; set; }
        public MMDTexture Texture { get; set; }
        public MMDTexture SubTexture { get; set; }
        public SubTextureTypeEnum SubTextureType { get; set; }


        public string MetaInfo { get; set; }
    }
}