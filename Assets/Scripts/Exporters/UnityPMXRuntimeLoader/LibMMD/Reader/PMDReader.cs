using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibMMD.Material;
using LibMMD.Model;
using LibMMD.Util;
using UnityEngine;

namespace LibMMD.Reader
{
    public class PMDReader : ModelReader
    {
        public override RawMMDModel Read(BinaryReader reader, ModelConfig config)
        {
            var model = new RawMMDModel();
            var context = new PmdReadContext();
            var meta = ReadMeta(reader);
            if (!"Pmd".Equals(meta.Magic) || Math.Abs(meta.Version - 1.0f) > 0.0001f)
            {
                throw new MMDFileParseException("File is not a PMD 1.0 file");
            }
            ReadDescription(reader, model);
            ReadVertices(reader, model);
            ReadTriangles(reader, model);
            var toonTextureIds = new List<int>();
            ReadParts(reader, model, toonTextureIds);
            ReadBonesAndIks(reader, model, context);
            ReadFaces(reader, model);
            ReadFacdDisplayListNames(reader);
            ReadBoneNameList(reader, context);
            ReadBoneDisp(reader);
            if (MMDReaderWriteUtil.Eof(reader))
            {
                goto PMD_READER_READ_LEGACY_30;
            }
            ReadInfoEn(reader, model, context);
            if (MMDReaderWriteUtil.Eof(reader))
            {
                goto PMD_READER_READ_LEGACY_30;
            }
            ReadCustomTextures(reader, config, model, toonTextureIds);
            if (MMDReaderWriteUtil.Eof(reader))
            {
                goto PMD_READER_READ_LEGACY_50;
            }
            ReadRigidBodies(reader, model, context);
            ReadConstraints(reader, model);
            goto PMD_READER_READ_SUCCEED;

            PMD_READER_READ_LEGACY_30:

            for (var i = 0; i < model.Parts.Length; ++i)
            {
                var material = model.Parts[i].Material;
                material.Toon = MMDTextureUtil.GetGlobalToon(toonTextureIds[i]);
            }
            PMD_READER_READ_LEGACY_50:
            PMD_READER_READ_SUCCEED:
            model.Normalize();
            return model;
        }

        private static void ReadConstraints(BinaryReader reader, RawMMDModel model)
        {
            var constraintNum = reader.ReadUInt32();
            model.Joints = new Model.MMDJoint[constraintNum];
            for (var i = 0; i < constraintNum; ++i)
            {
                var constraint = new Model.MMDJoint();
                constraint.Name = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding);
                constraint.AssociatedRigidBodyIndex[0] = reader.ReadInt32();
                constraint.AssociatedRigidBodyIndex[1] = reader.ReadInt32();
                constraint.Position = MMDReaderWriteUtil.ReadVector3(reader);
                constraint.Rotation = MMDReaderWriteUtil.ReadAmpVector3(reader, Mathf.Rad2Deg);
                constraint.PositionLowLimit = MMDReaderWriteUtil.ReadVector3(reader);
                constraint.PositionHiLimit = MMDReaderWriteUtil.ReadVector3(reader);
                constraint.RotationLowLimit = MMDReaderWriteUtil.ReadVector3(reader);
                constraint.RotationHiLimit = MMDReaderWriteUtil.ReadVector3(reader);
                constraint.SpringTranslate = MMDReaderWriteUtil.ReadVector3(reader);
                constraint.SpringRotate = MMDReaderWriteUtil.ReadVector3(reader);
                model.Joints[i] = constraint;
            }
        }

        private static void ReadRigidBodies(BinaryReader reader, RawMMDModel model, PmdReadContext context)
        {
            var rigidBodyNum = reader.ReadInt32();
            model.Rigidbodies = new MMDRigidBody[rigidBodyNum];
            for (var i = 0; i < rigidBodyNum; ++i)
            {
                var rigidBody = new MMDRigidBody();

                rigidBody.Name = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding).Trim();
                var boneIndex = reader.ReadUInt16();
                if (boneIndex < context.BoneNum)
                {
                    rigidBody.AssociatedBoneIndex = boneIndex;
                }
                else
                {
                    if (context.CenterBoneIndex == null)
                    {
                        rigidBody.AssociatedBoneIndex = 0;
                    }
                    else
                    {
                        rigidBody.AssociatedBoneIndex = context.CenterBoneIndex.Value;
                    }
                }
                rigidBody.CollisionGroup = reader.ReadSByte();
                rigidBody.CollisionMask = reader.ReadUInt16();
                rigidBody.Shape = (MMDRigidBody.RigidBodyShape) reader.ReadByte();
                rigidBody.Dimemsions = MMDReaderWriteUtil.ReadVector3(reader);
                var rbPosition = MMDReaderWriteUtil.ReadVector3(reader);
                rigidBody.Position = model.Bones[rigidBody.AssociatedBoneIndex].Position + rbPosition;
                rigidBody.Rotation = MMDReaderWriteUtil.ReadVector3(reader);
                rigidBody.Mass = reader.ReadSingle();
                rigidBody.TranslateDamp = reader.ReadSingle();
                rigidBody.RotateDamp = reader.ReadSingle();
                rigidBody.Restitution = reader.ReadSingle();
                rigidBody.Friction = reader.ReadSingle();

                var type = reader.ReadByte();
                if (boneIndex < context.BoneNum)
                {
                    rigidBody.Type = (MMDRigidBody.RigidBodyType) type;
                }
                else
                {
                    rigidBody.Type = MMDRigidBody.RigidBodyType.RigidTypePhysicsGhost;
                }
                model.Rigidbodies[i] = rigidBody;
            }
        }

        private static void ReadCustomTextures(BinaryReader reader, ModelConfig config, RawMMDModel model,
            List<int> toonTextureIds)
        {
            var customTextures = new MMDTexture[10];
            for (var i = 0; i < 10; ++i)
            {
                customTextures[i] =
                    new MMDTexture(MMDReaderWriteUtil.ReadStringFixedLength(reader, 100, Tools.JapaneseEncoding));
            }

            for (var i = 0; i < model.Parts.Length; ++i)
            {
                var material = model.Parts[i].Material;
                if (toonTextureIds[i] < 10)
                {
                    material.Toon = customTextures[toonTextureIds[i]];
                }
                else
                {
                    material.Toon = MMDTextureUtil.GetGlobalToon(0);
                }
            }
        }

        private static void ReadInfoEn(BinaryReader reader, RawMMDModel model, PmdReadContext context)
        {
            var hasInfoEn = reader.ReadSByte() == 1;
            if (!hasInfoEn) return;
            model.NameEn = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding);
            model.DescriptionEn = MMDReaderWriteUtil.ReadStringFixedLength(reader, 256, Tools.JapaneseEncoding);

            for (var i = 0; i < context.BoneNum; ++i)
            {
                var bone = model.Bones[i];
                bone.NameEn = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding);
            }

            if (model.Morphs.Length > 0)
            {
                model.Morphs[0].NameEn = model.Morphs[0].Name;
            }
            for (var i = 1; i < model.Morphs.Length; ++i)
            {
                var morph = model.Morphs[i];
                morph.NameEn = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding);
            }

            // UNDONE
            for (var i = 0; i < context.BoneNameListNum; ++i)
            {
                MMDReaderWriteUtil.ReadStringFixedLength(reader, 50, Tools.JapaneseEncoding);
            }
        }

        private static void ReadBoneDisp(BinaryReader reader)
        {
            var boneDispNum = reader.ReadInt32();
            for (var i = 0; i < boneDispNum; ++i)
            {
                reader.ReadUInt16();
                reader.ReadSByte();
            }
        }

        private static void ReadBoneNameList(BinaryReader reader, PmdReadContext context)
        {
            int boneNameListNum = reader.ReadSByte();
            context.BoneNameListNum = boneNameListNum;
            for (var i = 0; i < boneNameListNum; ++i)
            {
                MMDReaderWriteUtil.ReadStringFixedLength(reader, 50, Tools.JapaneseEncoding);
            }
        }

        private static void ReadFacdDisplayListNames(BinaryReader reader)
        {
            int faceDisplayListName = reader.ReadSByte();
            for (var i = 0; i < faceDisplayListName; ++i)
            {
                reader.ReadUInt16();
            }
        }

        private void ReadFaces(BinaryReader reader, RawMMDModel model)
        {
            var faceNum = reader.ReadUInt16();
            int? baseMorphIndex = null;
            model.Morphs = new Morph[faceNum];
            for (var i = 0; i < faceNum; ++i)
            {
                var morph = new Morph();
                var fp = ReadPmdFacePreamble(reader);
                morph.Name = fp.Name;
                morph.Category = (Morph.MorphCategory) fp.FaceType;
                if (morph.Category == Morph.MorphCategory.MorphCatSystem)
                {
                    baseMorphIndex = i;
                }
                morph.Type = Morph.MorphType.MorphTypeVertex;
                morph.MorphDatas = new Morph.MorphData[fp.VertexNum];
                for (var j = 0; j < fp.VertexNum; ++j)
                {
                    var vertexMorphData = new Morph.VertexMorphData();
                    vertexMorphData.VertexIndex = reader.ReadInt32();
                    vertexMorphData.Offset = MMDReaderWriteUtil.ReadVector3(reader);
                    morph.MorphDatas[j] = vertexMorphData;
                }
                model.Morphs[i] = morph;
            }

            if (baseMorphIndex != null)
            {
                var baseMorph = model.Morphs[baseMorphIndex.Value];
                for (var i = 0; i < faceNum; ++i)
                {
                    if (i == baseMorphIndex)
                    {
                        continue;
                    }
                    var morph = model.Morphs[i];
                    for (var j = 0; j < morph.MorphDatas.Length; ++j)
                    {
                        var vertexMorphData = (Morph.VertexMorphData) morph.MorphDatas[j];
                        var morphDataVertexIndex = vertexMorphData.VertexIndex;
                        vertexMorphData.VertexIndex = ((Morph.VertexMorphData) baseMorph.MorphDatas[morphDataVertexIndex])
                            .VertexIndex;
                    }
                }
            }
        }

        private PmdFacePreamble ReadPmdFacePreamble(BinaryReader reader)
        {
            return new PmdFacePreamble
            {
                Name = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding),
                VertexNum = reader.ReadInt32(),
                FaceType = reader.ReadSByte()
            };
        }

        private void ReadBonesAndIks(BinaryReader reader, RawMMDModel model, PmdReadContext context)
        {
            int boneNum = reader.ReadUInt16();
            context.BoneNum = boneNum;
            var rawBones = new PmdBone[boneNum];
            for (var i = 0; i < boneNum; ++i)
            {
                rawBones[i] = ReadPmdBone(reader);
            }

            var ikBoneIds = new HashSet<int>();
            int ikNum = reader.ReadUInt16();
            var rawIkList = new List<PmdRawIk>(ikNum);
            for (var i = 0; i < ikNum; ++i)
            {
                var rawIk = new PmdRawIk();
                rawIk.Preamble = ReadPmdIkPreamable(reader);
                ikBoneIds.Add(rawIk.Preamble.IkBoneIndex);
                rawIk.Chain = new int[rawIk.Preamble.IkChainLength];
                for (var j = 0; j < rawIk.Preamble.IkChainLength; ++j)
                {
                    rawIk.Chain[j] = reader.ReadUInt16();
                }
                rawIkList.Add(rawIk);
            }

            rawIkList.Sort((ik1, ik2) =>
            {
                var a1 = 0;
                var a2 = 0;
                if (ik1.Chain.Length > 0)
                {
                    a1 = ik1.Chain[0];
                }
                if (ik2.Chain.Length > 0)
                {
                    a2 = ik2.Chain[0];
                }
                return a1.CompareTo(a2);
            });

            var boneList = new List<Bone>();

            int? centerBoneIndex = null;
            model.Bones = new Bone[boneNum];
            for (var i = 0; i < boneNum; ++i)
            {
                var bone = new Bone();
                boneList.Add(bone);
                var rawBone = rawBones[i];
                bone.Name = rawBone.Name;
                if ("\u30BB\u30F3\u30BF\u30FC".Equals(bone.Name)) //TODO 验证是不是这个值
                {
                    centerBoneIndex = i;
                }
                bone.Position = rawBone.Position;
                // TODO - workaround here, need fix, see [1].
                if (i != rawBone.ParentId)
                {
                    bone.ParentIndex = rawBone.ParentId;
                }
                else
                {
                    bone.ParentIndex = 0; //TODO mmdlib里是nil
                }
                bone.TransformLevel = 0;
                bone.ChildBoneVal.ChildUseId = true;
                bone.ChildBoneVal.Index = rawBone.ChildId;
                bone.Rotatable = true;
                var type = (PmdBoneTypes) rawBone.Type;
                bone.HasIk = type == PmdBoneTypes.PmdBoneIk || ikBoneIds.Contains(i);
                bone.Movable = type == PmdBoneTypes.PmdBoneRotateAndTranslate || bone.HasIk;
                bone.Visible = type != PmdBoneTypes.PmdBoneIkTo && type != PmdBoneTypes.PmdBoneInvisible &&
                               type != PmdBoneTypes.PmdBoneRotateRatio;
                bone.Controllable = true;
                bone.AppendRotate = type == PmdBoneTypes.PmdBoneRotateEffect || type == PmdBoneTypes.PmdBoneRotateRatio;
                bone.AppendTranslate = false;
                bone.RotAxisFixed = type == PmdBoneTypes.PmdBoneTwist;
                bone.UseLocalAxis = false;
                bone.PostPhysics = false;
                bone.ReceiveTransform = false;

                if (bone.AppendRotate)
                {
                    if (type == PmdBoneTypes.PmdBoneRotateEffect)
                    {
                        bone.AppendBoneVal.Index = rawBone.IkNumber;
                        bone.AppendBoneVal.Ratio = 1.0f;
                        bone.TransformLevel = 2;
                    }
                    else
                    {
                        bone.ChildBoneVal.ChildUseId = false;
                        bone.ChildBoneVal.Offset = Vector3.zero;
                        bone.AppendBoneVal.Index = rawBone.ChildId;
                        bone.AppendBoneVal.Ratio = rawBone.IkNumber * 0.01f;
                    }
                }
                if (bone.HasIk)
                {
                    bone.TransformLevel = 1;
                }

                if (bone.RotAxisFixed)
                {
                    var childId = rawBone.ChildId;
                    if (childId > boneNum)
                    {
                        childId = 0;
                    }
                    bone.RotAxis = (rawBones[childId].Position - bone.Position).normalized;
                    if (bone.ChildBoneVal.ChildUseId)
                    {
                        bone.ChildBoneVal.ChildUseId = false;
                        bone.ChildBoneVal.Offset = Vector3.zero;
                    }
                }

                model.Bones[i] = bone;
            }

            var loLimit = Vector3.zero;
            var hiLimit = Vector3.zero;
            loLimit.x = (float) -Math.PI;
            hiLimit.x = (float) (-0.5f / 180.0f * Math.PI);

            for (var i = 0; i < boneNum; ++i)
            {
                if (!ikBoneIds.Contains(i))
                {
                    boneList[i].IkInfoVal = new Bone.IkInfo
                    {
                        IkLinks = new Bone.IkLink[0]
                    };
                    continue;
                }
                var associatedIkCount = 0;
                for (var j = 0; j < ikNum; ++j)
                {
                    var rawIk = rawIkList[j];
                    if (i != rawIk.Preamble.IkBoneIndex)
                    {
                        continue;
                    }
                    Bone bone;
                    if (associatedIkCount == 0)
                    {
                        bone = boneList[i];
                    }
                    else
                    {
                        var originalBone = boneList[i];
                        bone = Bone.CopyOf(originalBone);
                        boneList.Add(bone);
                        bone.Name = "[IK]" + originalBone.Name;
                        bone.NameEn = "[IK]" + originalBone.NameEn;
                        bone.ParentIndex = i;
                        bone.ChildBoneVal.ChildUseId = false;
                        bone.ChildBoneVal.Offset = Vector3.zero;
                        bone.Visible = false;
                        bone.IkInfoVal.IkLinks = new Bone.IkLink[rawIk.Preamble.IkChainLength];
                        bone.HasIk = true;
                    }
                    bone.IkInfoVal = new Bone.IkInfo();
                    bone.IkInfoVal.IkTargetIndex = rawIk.Preamble.IkTargetBoneIndex;
                    bone.IkInfoVal.CcdIterateLimit = rawIk.Preamble.CcdIterateLimit;
                    bone.IkInfoVal.CcdAngleLimit = rawIk.Preamble.CcdAngleLimit;
                    bone.IkInfoVal.IkLinks = new Bone.IkLink[rawIk.Preamble.IkChainLength];

                    for (var k = 0; k < rawIk.Preamble.IkChainLength; ++k)
                    {
                        var link = new Bone.IkLink();
                        link.LinkIndex = rawIk.Chain[k];
                        var linkName = model.Bones[link.LinkIndex].Name;
                        if ("\u5DE6\u3072\u3056".Equals(linkName) || "\u53F3\u3072\u3056".Equals(linkName))
                        {
                            link.HasLimit = true;
                            link.LoLimit = loLimit;
                            link.HiLimit = hiLimit;
                        }
                        else
                        {
                            link.HasLimit = false;
                        }

                        bone.IkInfoVal.IkLinks[k] = link;
                    }

                    associatedIkCount++;
                }
            }

            model.Bones = boneList.ToArray();

            // TODO - need verification

            for (var i = 0; i < boneNum; ++i)
            {
                var stable = true;
                for (var j = 0; j < boneNum; ++j)
                {
                    var bone = model.Bones[j];
                    var transformLevel = bone.TransformLevel;
                    var parentId = bone.ParentIndex;
                    while (parentId < boneNum)
                    {
                        var parentTransformLevel = model.Bones[parentId].TransformLevel;
                        if (transformLevel < parentTransformLevel)
                        {
                            transformLevel = parentTransformLevel;
                            stable = false;
                        }
                        parentId = model.Bones[parentId].ParentIndex;
                    }
                    bone.TransformLevel = transformLevel;
                }
                if (stable)
                {
                    break;
                }
            }
            context.CenterBoneIndex = centerBoneIndex;
        }

        private static void ReadParts(BinaryReader reader, RawMMDModel model, List<int> tooTextureIds)
        {
            var partNum = reader.ReadInt32();
            var partBaseShift = 0;
            model.Parts = new Part[partNum];
            for (var i = 0; i < partNum; ++i)
            {
                var material = new MMDMaterial();
                material.DiffuseColor = MMDReaderWriteUtil.ReadColor(reader, true);
                material.Shiness = reader.ReadSingle();
                material.SpecularColor = MMDReaderWriteUtil.ReadColor(reader, false);
                material.AmbientColor = MMDReaderWriteUtil.ReadColor(reader, false);
                var toonId = reader.ReadByte();
                var edgeFlag = reader.ReadSByte();
                var vertexNum = reader.ReadInt32();
                var textureName = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding);
                material.DrawDoubleFace = material.DiffuseColor.a < 0.9999f;
                material.DrawGroundShadow = edgeFlag != 0;
                material.CastSelfShadow = true;
                material.DrawSelfShadow = true;
                material.DrawEdge = edgeFlag != 0;
                material.EdgeColor = Color.black;
                if (!string.IsNullOrEmpty(textureName))
                {
                    var dlmPos = textureName.IndexOf("*", StringComparison.Ordinal);
                    if (dlmPos >= 0)
                    {
                        var texPath = textureName.Substring(0, dlmPos);
                        var sphPath = textureName.Substring(dlmPos + 1);
                        if (!string.IsNullOrEmpty(texPath))
                        {
                            material.Texture = new MMDTexture(texPath);
                        }
                        if (!string.IsNullOrEmpty(sphPath))
                        {
                            material.SubTexture = new MMDTexture(sphPath);
                        }
                        if (sphPath.EndsWith("a") || sphPath.EndsWith("A"))
                        {
                            material.SubTextureType = MMDMaterial.SubTextureTypeEnum.MatSubTexSpa;
                        }
                        else
                        {
                            material.SubTextureType = MMDMaterial.SubTextureTypeEnum.MatSubTexSph;
                        }
                    }
                    else
                    {
                        var extDlmPos = textureName.LastIndexOf(".", StringComparison.Ordinal);
                        if (extDlmPos >= 0)
                        {
                            var ext = textureName.Substring(extDlmPos + 1);
                            ext = ext.ToLower();
                            if (!ext.Equals("sph") && !ext.Equals("spa"))
                            {
                                material.Texture = new MMDTexture(textureName);
                            }
                            else
                            {
                                material.SubTexture = new MMDTexture(textureName);
                                ;
                                if (ext.EndsWith("a"))
                                {
                                    material.SubTextureType = MMDMaterial.SubTextureTypeEnum.MatSubTexSpa;
                                }
                                else
                                {
                                    material.SubTextureType = MMDMaterial.SubTextureTypeEnum.MatSubTexSph;
                                }
                            }
                        }
                        else
                        {
                            material.Texture = new MMDTexture(textureName);
                        }
                    }
                }
                var part = new Part();
                part.Material = material;
                tooTextureIds.Add(toonId);
                part.BaseShift = partBaseShift;
                part.TriangleIndexNum = vertexNum;
                partBaseShift += vertexNum;
                model.Parts[i] = part;
            }
        }

        private static void ReadTriangles(BinaryReader reader, RawMMDModel model)
        {
            var triangleIndexCount = reader.ReadInt32();
            model.TriangleIndexes = new int[triangleIndexCount];
            if (triangleIndexCount % 3 != 0)
            {
                throw new MMDFileParseException("triangle index count " + triangleIndexCount + " is not multiple of 3");
            }
            for (var i = 0; i < triangleIndexCount; ++i)
            {
                model.TriangleIndexes[i] = reader.ReadUInt16();
            }
        }

        private static void ReadVertices(BinaryReader reader, RawMMDModel model)
        {
            var vertexNum = reader.ReadInt32();
            model.Vertices = new Vertex[vertexNum];
            for (var i = 0; i < vertexNum; ++i)
            {
                var vertex = new Vertex
                {
                    Coordinate = MMDReaderWriteUtil.ReadVector3(reader),
                    Normal = MMDReaderWriteUtil.ReadVector3(reader),
                    UvCoordinate = MMDReaderWriteUtil.ReadVector2(reader)
                };
                var skinningOperator = new SkinningOperator();
                var bdef2 = new SkinningOperator.Bdef2();
                bdef2.BoneId = new int[2];
                bdef2.BoneId[0] = reader.ReadInt16();
                bdef2.BoneId[1] = reader.ReadInt16();
                bdef2.BoneWeight = reader.ReadSByte() * 0.01f;
                skinningOperator.Param = bdef2;
                skinningOperator.Type = SkinningOperator.SkinningType.SkinningBdef2;
                vertex.SkinningOperator = skinningOperator;
                var noEdge = reader.ReadByte() != 0;
                vertex.EdgeScale = noEdge ? 0.0f : 1.0f;
                model.Vertices[i] = vertex;
            }
        }


        private static void ReadDescription(BinaryReader reader, RawMMDModel model)
        {
            model.Name = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding);
            model.Description = MMDReaderWriteUtil.ReadStringFixedLength(reader, 256, Tools.JapaneseEncoding);
        }

        private static PmdMeta ReadMeta(BinaryReader reader)
        {
            PmdMeta ret;
            ret.Magic = MMDReaderWriteUtil.ReadStringFixedLength(reader, 3, Encoding.ASCII);
            ret.Version = reader.ReadSingle();
            return ret;
        }

        private struct PmdMeta
        {
            public string Magic;
            public float Version;
        }

        private PmdBone ReadPmdBone(BinaryReader reader)
        {
            return new PmdBone
            {
                Name = MMDReaderWriteUtil.ReadStringFixedLength(reader, 20, Tools.JapaneseEncoding),
                ParentId = reader.ReadUInt16(),
                ChildId = reader.ReadUInt16(),
                Type = reader.ReadSByte(),
                IkNumber = reader.ReadUInt16(),
                Position = MMDReaderWriteUtil.ReadVector3(reader)
            };
        }

        private PmdIkPreamble ReadPmdIkPreamable(BinaryReader reader)
        {
            return new PmdIkPreamble
            {
                IkBoneIndex = reader.ReadInt16(),
                IkTargetBoneIndex = reader.ReadInt16(),
                IkChainLength = reader.ReadSByte(),
                CcdIterateLimit = reader.ReadUInt16(),
                CcdAngleLimit = reader.ReadSingle()
            };
        }


        private class PmdBone
        {
            public string Name { get; set; }
            public ushort ParentId { get; set; }
            public ushort ChildId { get; set; }
            public sbyte Type { get; set; }
            public ushort IkNumber { get; set; }
            public Vector3 Position { get; set; }
        }

        private class PmdIkPreamble
        {
            public short IkBoneIndex { get; set; }
            public short IkTargetBoneIndex { get; set; }
            public sbyte IkChainLength { get; set; }
            public ushort CcdIterateLimit { get; set; }
            public float CcdAngleLimit { get; set; }
        }

        private class PmdRawIk
        {
            public PmdIkPreamble Preamble { get; set; }
            public int[] Chain { get; set; }
        }

        private enum PmdBoneTypes : byte
        {
            PmdBoneRotate = 0,
            PmdBoneRotateAndTranslate = 1,
            PmdBoneIk = 2,
            PmdBoneUnknown = 3,
            PmdBoneIkLink = 4,
            PmdBoneRotateEffect = 5,
            PmdBoneIkTo = 6,
            PmdBoneInvisible = 7,
            PmdBoneTwist = 8,
            PmdBoneRotateRatio = 9
        }

        private class PmdFacePreamble
        {
            public string Name { get; set; }
            public int VertexNum { get; set; }
            public sbyte FaceType { get; set; }
        }

        private class PmdReadContext
        {
            public int? CenterBoneIndex { get; set; }
            public int BoneNum { get; set; }
            public int BoneNameListNum { get; set; }
        }
    }
}