using LibMMD.Material;
using System;
using System.Collections.Generic;
using static LibMMD.Reader.PMXReader;

namespace LibMMD.Model
{
    public class RawMMDModel
    {
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public string DescriptionEn { get; set; }
        public Vertex[] Vertices { get; set; }
        public int ExtraUvNumber { get; set; }
        public int[] TriangleIndexes { get; set; }
        public Part[] Parts { get; set; }
        public Bone[] Bones { get; set; }
        public Morph[] Morphs { get; set; }
        public MMDRigidBody[] Rigidbodies { get; set; }
        public MMDJoint[] Joints { get; set; }

        public List<MMDTexture> TextureList = new List<MMDTexture>();
        public List<PMXEntryItem> Entrys = new List<PMXEntryItem>();

        public void Normalize()
        {
            foreach (var vertex in Vertices)
            {
                switch (vertex.SkinningOperator.Type)
                {
                    case SkinningOperator.SkinningType.SkinningBdef2:
                    {
                        var oldBdef2 = (SkinningOperator.Bdef2) vertex.SkinningOperator.Param;
                        var weight = oldBdef2.BoneWeight;
                        if (Math.Abs(weight) < 0.000001f)
                        {
                            var bdef1 = new SkinningOperator.Bdef1 {BoneId = oldBdef2.BoneId[1]};
                            vertex.SkinningOperator.Param = bdef1;
                            vertex.SkinningOperator.Type = SkinningOperator.SkinningType.SkinningBdef1;
                        }
                        else if (Math.Abs(weight - 1.0f) < 0.00001f)
                        {
                            var bdef1 = new SkinningOperator.Bdef1 {BoneId = oldBdef2.BoneId[0]};
                            vertex.SkinningOperator.Param = bdef1;
                            vertex.SkinningOperator.Type = SkinningOperator.SkinningType.SkinningBdef1;
                        }
                        break;
                    }
                    case SkinningOperator.SkinningType.SkinningSdef:
                    {
                        var oldSdef = (SkinningOperator.Sdef) vertex.SkinningOperator.Param;
                        var bone0 = oldSdef.BoneId[0];
                        var bone1 = oldSdef.BoneId[1];
                        var weight = oldSdef.BoneWeight;
                        if (
                            Bones[bone0].ParentIndex != bone1 &&
                            Bones[bone1].ParentIndex != bone0
                        )
                        {
                            if (Math.Abs(weight) < 0.000001f)
                            {
                                var bdef1 = new SkinningOperator.Bdef1 {BoneId = bone1};
                                vertex.SkinningOperator.Param = bdef1;
                                vertex.SkinningOperator.Type = SkinningOperator.SkinningType.SkinningBdef1;
                            }
                            else if (Math.Abs(weight - 1.0f) < 0.00001f)
                            {
                                var bdef1 = new SkinningOperator.Bdef1 {BoneId = bone0};
                                vertex.SkinningOperator.Param = bdef1;
                                vertex.SkinningOperator.Type = SkinningOperator.SkinningType.SkinningBdef1;
                            }
                            else
                            {
                                var bdef2 = new SkinningOperator.Bdef2();
                                bdef2.BoneId[0] = bone0;
                                bdef2.BoneId[1] = bone1;
                                bdef2.BoneWeight = weight;
                                vertex.SkinningOperator.Param = bdef2;
                                vertex.SkinningOperator.Type = SkinningOperator.SkinningType.SkinningBdef2;
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    public class PMXEntryItem
    {
        public string EntryItemName;
        public string EntryItemNameEn;
        public bool IsSpecial;
        public List<Element> Elements;

        public class Element 
        {
            public bool IsMorph;
            public int MorphIndex;
            public int BoneIndex;
        }
    }
}