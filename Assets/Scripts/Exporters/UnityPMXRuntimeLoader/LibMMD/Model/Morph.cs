using System.Collections.Generic;
using LibMMD.Util;
using UnityEngine;

namespace LibMMD.Model
{
    public class Morph
    {
        public enum MorphCategory : byte
        {
            MorphCatSystem = 0x00,
            MorphCatEyebrow = 0x01,
            MorphCatEye = 0x02,
            MorphCatMouth = 0x03,
            MorphCatOther = 0x04
        }

        public enum MorphType : byte
        {
            MorphTypeGroup = 0x00,
            MorphTypeVertex = 0x01,
            MorphTypeBone = 0x02,
            MorphTypeUv = 0x03,
            MorphTypeExtUv1 = 0x04,
            MorphTypeExtUv2 = 0x05,
            MorphTypeExtUv3 = 0x06,
            MorphTypeExtUv4 = 0x07,
            MorphTypeMaterial = 0x08
        }

        public abstract class MorphData
        {
        }

        public class GroupMorphData : MorphData
        {
            public int MorphIndex { get; set; }
            public float MorphRate { get; set; }
        }

        public class VertexMorphData : MorphData
        {
            public int VertexIndex { get; set; }
            public Vector3 Offset { get; set; }
        }

        public class BoneMorphData : MorphData
        {
            public int BoneIndex { get; set; }
            public Vector3 Translation { get; set; }
            public Quaternion Rotation { get; set; }
        }

        public class UvMorphData : MorphData
        {
            public int VertexIndex { get; set; }
            public Vector4 Offset { get; set; }
        }

        public class MaterialMorphData : MorphData
        {
            public enum MaterialMorphMethod : byte {
                MorphMatMul = 0x00,
                MorphMatAdd = 0x01
            }

            public int MaterialIndex { get; set; }
            public bool Global { get; set; }
            public MaterialMorphMethod Method { get; set; }
            public Color Diffuse { get; set; }
            public Color Specular { get; set; }
            public Color Ambient { get; set; }
            public float Shiness { get; set; }
            public Color EdgeColor { get; set; }
            public float EdgeSize { get; set; }
            public Vector4 Texture { get; set; }
            public Vector4 SubTexture { get; set; }
            public Vector4 ToonTexture { get; set; }
        }

        public string Name { get; set; }
        public string NameEn { get; set; }
        public MorphCategory Category { get; set; }
        public MorphType Type { get; set; }
        public MorphData[] MorphDatas { get; set; }
    }
}