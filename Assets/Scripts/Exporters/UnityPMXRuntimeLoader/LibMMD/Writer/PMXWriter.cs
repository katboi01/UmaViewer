using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibMMD.Material;
using LibMMD.Model;
using LibMMD.Reader;
using LibMMD.Util;
using UnityEngine;
using static LibMMD.Reader.PMXReader;

namespace LibMMD.Writer
{
    public class PMXWriter
    {
        public static void Write(BinaryWriter writer, RawMMDModel model, ModelConfig config)
        {
            var pmxHeader = new PmxMeta
            {
                Magic = "PMX ",
                Version = 2,
                FileFlagSize = 8
            };

            PmxConfig PmxConfig = new PmxConfig()
            {
                Utf8Encoding = false,
                Encoding = Encoding.Unicode,
                ExtraUvNumber = 3, //include uv2 and color
                VertexIndexSize = 2,
                TextureIndexSize = 1,
                MaterialIndexSize = 1,
                BoneIndexSize = 2,
                MorphIndexSize = 2,
                RigidBodyIndexSize = 2
            };

            PMXEntryItem.Element baseElement = new PMXEntryItem.Element();
            baseElement.IsMorph = false;
            baseElement.MorphIndex = 0;
            baseElement.BoneIndex = 0;

            PMXEntryItem baseEntry = new PMXEntryItem();
            baseEntry.EntryItemName = "Root";
            baseEntry.EntryItemNameEn = "Root";
            baseEntry.IsSpecial = true;
            baseEntry.Elements = new List<PMXEntryItem.Element>{ baseElement };

            model.Entrys.Insert(0, baseEntry);

            WriteMeta(writer, pmxHeader);
            WritePmxConfig(writer, PmxConfig);
            WriteModelNameAndDescription(writer, model, PmxConfig);
            WriteVertices(writer, model, PmxConfig);
            WriteTriangles(writer, model, PmxConfig);
            WriteTextureList(writer, model.TextureList, PmxConfig);
            WriteParts(writer, config, model, PmxConfig);
            WriteBones(writer, model, PmxConfig);
            WriteMorphs(writer, model, PmxConfig);
            WriteEntries(writer, model.Entrys, PmxConfig);
            WriteRigidBodies(writer, model.Rigidbodies, PmxConfig);
            WriteJoints(writer, model, PmxConfig);
        }

        private static void WriteJoints(BinaryWriter writer, RawMMDModel model, PmxConfig pmxConfig)
        {
            writer.Write(model.Joints.Length); // constraintNum
            foreach (var joint in model.Joints)
            {
                MMDReaderWriteUtil.WriteSizedString(writer, joint.Name, pmxConfig.Encoding);
                MMDReaderWriteUtil.WriteSizedString(writer, joint.NameEn, pmxConfig.Encoding);

                writer.Write((byte)0); // dofType

                MMDReaderWriteUtil.WriteIndex(writer, joint.AssociatedRigidBodyIndex[0], pmxConfig.RigidBodyIndexSize);
                MMDReaderWriteUtil.WriteIndex(writer, joint.AssociatedRigidBodyIndex[1], pmxConfig.RigidBodyIndexSize);
                MMDReaderWriteUtil.WriteVector3(writer, joint.Position);
                MMDReaderWriteUtil.WriteAmpVector3(writer, joint.Rotation, Mathf.Rad2Deg);
                MMDReaderWriteUtil.WriteVector3(writer, joint.PositionLowLimit);
                MMDReaderWriteUtil.WriteVector3(writer, joint.PositionHiLimit);
                MMDReaderWriteUtil.WriteVector3(writer, joint.RotationLowLimit, false);
                MMDReaderWriteUtil.WriteVector3(writer, joint.RotationHiLimit, false);
                MMDReaderWriteUtil.WriteVector3(writer, joint.SpringTranslate);
                MMDReaderWriteUtil.WriteVector3(writer, joint.SpringRotate, false);
            }
        }

        private static void WriteRigidBodies(BinaryWriter writer, MMDRigidBody[] rigidBodies, PmxConfig pmxConfig)
        {
            writer.Write(rigidBodies.Length); // rigidBodyNum
            foreach (var rigidBody in rigidBodies)
            {
                MMDReaderWriteUtil.WriteSizedString(writer, rigidBody.Name, pmxConfig.Encoding); // Name
                MMDReaderWriteUtil.WriteSizedString(writer, rigidBody.NameEn, pmxConfig.Encoding); // NameEn
                MMDReaderWriteUtil.WriteIndex(writer, rigidBody.AssociatedBoneIndex, pmxConfig.BoneIndexSize); // AssociatedBoneIndex
                writer.Write(rigidBody.CollisionGroup); // CollisionGroup
                writer.Write(rigidBody.CollisionMask); // CollisionMask
                writer.Write((byte)rigidBody.Shape); // Shape
                MMDReaderWriteUtil.WriteRawCoordinateVector3(writer, rigidBody.Dimemsions); // Dimemsions
                MMDReaderWriteUtil.WriteVector3(writer, rigidBody.Position); // Position
                MMDReaderWriteUtil.WriteAmpVector3(writer, rigidBody.Rotation, Mathf.Deg2Rad); // Rotation
                writer.Write(rigidBody.Mass); // Mass
                writer.Write(rigidBody.TranslateDamp); // TranslateDamp
                writer.Write(rigidBody.RotateDamp); // RotateDamp
                writer.Write(rigidBody.Restitution); // Restitution
                writer.Write(rigidBody.Friction); // Friction
                writer.Write((byte)rigidBody.Type); // Type
            }
        }

        private static void WriteEntries(BinaryWriter writer, List<PMXEntryItem> entryItems, PmxConfig pmxConfig)
        {
            writer.Write(entryItems.Count); // entryItemNum
            foreach (var entryItem in entryItems)
            {
                MMDReaderWriteUtil.WriteSizedString(writer, entryItem.EntryItemName, pmxConfig.Encoding); // entryItemName
                MMDReaderWriteUtil.WriteSizedString(writer, entryItem.EntryItemNameEn, pmxConfig.Encoding); // entryItemNameEn
                writer.Write((byte)(entryItem.IsSpecial ? 1 : 0)); // isSpecial
                writer.Write(entryItem.Elements.Count); // elementNum
                foreach (var element in entryItem.Elements)
                {
                    writer.Write((byte)(element.IsMorph ? 1 : 0)); // isMorph
                    if (element.IsMorph)
                    {
                        MMDReaderWriteUtil.WriteIndex(writer, element.MorphIndex, pmxConfig.MorphIndexSize); // morphIndex
                    }
                    else
                    {
                        MMDReaderWriteUtil.WriteIndex(writer, element.BoneIndex, pmxConfig.BoneIndexSize); // boneIndex
                    }
                }
            }
        }


        private static void WriteMorphs(BinaryWriter writer, RawMMDModel model, PmxConfig pmxConfig)
        {
            var morphNum = model.Morphs.Length;
            writer.Write(morphNum);

            int? baseMorphIndex = null;

            for (var i = 0; i < morphNum; ++i)
            {
                var morph = model.Morphs[i];

                MMDReaderWriteUtil.WriteSizedString(writer, morph.Name, pmxConfig.Encoding);
                MMDReaderWriteUtil.WriteSizedString(writer, morph.NameEn, pmxConfig.Encoding);
                writer.Write((byte)morph.Category);

                if (morph.Category == Morph.MorphCategory.MorphCatSystem)
                {
                    baseMorphIndex = i;
                }

                writer.Write((byte)morph.Type);

                var morphDataNum = morph.MorphDatas.Length;
                writer.Write(morphDataNum);

                switch (morph.Type)
                {
                    case Morph.MorphType.MorphTypeGroup:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData = (Morph.GroupMorphData)morph.MorphDatas[j];
                            MMDReaderWriteUtil.WriteIndex(writer, morphData.MorphIndex, pmxConfig.MorphIndexSize);
                            writer.Write(morphData.MorphRate);
                        }
                        break;
                    case Morph.MorphType.MorphTypeVertex:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData = (Morph.VertexMorphData)morph.MorphDatas[j];
                            MMDReaderWriteUtil.WriteIndex(writer, morphData.VertexIndex, pmxConfig.VertexIndexSize);
                            MMDReaderWriteUtil.WriteVector3(writer, morphData.Offset);
                        }
                        break;
                    case Morph.MorphType.MorphTypeBone:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData = (Morph.BoneMorphData)morph.MorphDatas[j];
                            MMDReaderWriteUtil.WriteIndex(writer, morphData.BoneIndex, pmxConfig.BoneIndexSize);
                            MMDReaderWriteUtil.WriteVector3(writer, morphData.Translation);
                            MMDReaderWriteUtil.WriteQuaternion(writer, morphData.Rotation);
                        }
                        break;
                    case Morph.MorphType.MorphTypeUv:
                    case Morph.MorphType.MorphTypeExtUv1:
                    case Morph.MorphType.MorphTypeExtUv2:
                    case Morph.MorphType.MorphTypeExtUv3:
                    case Morph.MorphType.MorphTypeExtUv4:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData = (Morph.UvMorphData)morph.MorphDatas[j];
                            MMDReaderWriteUtil.WriteIndex(writer, morphData.VertexIndex, pmxConfig.VertexIndexSize);
                            MMDReaderWriteUtil.WriteVector4(writer, morphData.Offset);
                        }
                        break;
                    case Morph.MorphType.MorphTypeMaterial:
                        for (var j = 0; j < morphDataNum; j++)
                        {
                            var morphData = (Morph.MaterialMorphData)morph.MorphDatas[j];

                            if (morphData.Global)
                            {
                                MMDReaderWriteUtil.WriteIndex(writer, 0, pmxConfig.MaterialIndexSize);
                            }
                            else
                            {
                                MMDReaderWriteUtil.WriteIndex(writer, morphData.MaterialIndex, pmxConfig.MaterialIndexSize);
                            }

                            writer.Write((byte)morphData.Method);
                            MMDReaderWriteUtil.WriteColor(writer, morphData.Diffuse, true);
                            MMDReaderWriteUtil.WriteColor(writer, morphData.Specular, false);
                            writer.Write(morphData.Shiness);
                            MMDReaderWriteUtil.WriteColor(writer, morphData.Ambient, false);
                            MMDReaderWriteUtil.WriteColor(writer, morphData.EdgeColor, true);
                            writer.Write(morphData.EdgeSize);
                            MMDReaderWriteUtil.WriteVector4(writer, morphData.Texture);
                            MMDReaderWriteUtil.WriteVector4(writer, morphData.SubTexture);
                            MMDReaderWriteUtil.WriteVector4(writer, morphData.ToonTexture);
                        }
                        break;
                    default:
                        throw new MMDFileParseException("invalid morph type " + morph.Type);
                }

                if (baseMorphIndex != null)
                {
                    // TODO: rectify system-reserved category
                }
            }
        }


        private static void WriteBones(BinaryWriter writer, RawMMDModel model, PmxConfig pmxConfig)
        {
            var boneNum = model.Bones.Length;
            writer.Write(boneNum);
            for (var i = 0; i < boneNum; ++i)
            {
                var bone = model.Bones[i];
                MMDReaderWriteUtil.WriteSizedString(writer, bone.Name, pmxConfig.Encoding);
                MMDReaderWriteUtil.WriteSizedString(writer, bone.NameEn, pmxConfig.Encoding);
                MMDReaderWriteUtil.WriteVector3(writer, bone.Position);
                MMDReaderWriteUtil.WriteIndex(writer, bone.ParentIndex, pmxConfig.BoneIndexSize);
                writer.Write(bone.TransformLevel);

                ushort flag = 0;
                if (bone.ChildBoneVal.ChildUseId)
                    flag |= PmxBoneFlags.PmxBoneChildUseId;
                if (bone.Rotatable)
                    flag |= PmxBoneFlags.PmxBoneRotatable;
                if (bone.Movable)
                    flag |= PmxBoneFlags.PmxBoneMovable;
                if (bone.Visible)
                    flag |= PmxBoneFlags.PmxBoneVisible;
                if (bone.Controllable)
                    flag |= PmxBoneFlags.PmxBoneControllable;
                if (bone.HasIk)
                    flag |= PmxBoneFlags.PmxBoneHasIk;
                if (bone.AppendRotate)
                    flag |= PmxBoneFlags.PmxBoneAcquireRotate;
                if (bone.AppendTranslate)
                    flag |= PmxBoneFlags.PmxBoneAcquireTranslate;
                if (bone.RotAxisFixed)
                    flag |= PmxBoneFlags.PmxBoneRotAxisFixed;
                if (bone.UseLocalAxis)
                    flag |= PmxBoneFlags.PmxBoneUseLocalAxis;
                if (bone.PostPhysics)
                    flag |= PmxBoneFlags.PmxBonePostPhysics;
                if (bone.ReceiveTransform)
                    flag |= PmxBoneFlags.PmxBoneReceiveTransform;
                writer.Write(flag);

                if (bone.ChildBoneVal.ChildUseId)
                {
                    MMDReaderWriteUtil.WriteIndex(writer, bone.ChildBoneVal.Index, pmxConfig.BoneIndexSize);
                }
                else
                {
                    MMDReaderWriteUtil.WriteVector3(writer, bone.ChildBoneVal.Offset);
                }

                if (bone.RotAxisFixed)
                {
                    MMDReaderWriteUtil.WriteVector3(writer, bone.RotAxis, false);
                }

                if (bone.AppendRotate || bone.AppendTranslate)
                {
                    MMDReaderWriteUtil.WriteIndex(writer, bone.AppendBoneVal.Index, pmxConfig.BoneIndexSize);
                    writer.Write(bone.AppendBoneVal.Ratio);
                }

                if (bone.UseLocalAxis)
                {
                    MMDReaderWriteUtil.WriteVector3(writer, bone.LocalAxisVal.AxisX, false);
                    MMDReaderWriteUtil.WriteVector3(writer, bone.LocalAxisVal.AxisZ, false);
                }

                if (bone.ReceiveTransform)
                {
                    writer.Write(bone.ExportKey);
                }

                if (bone.HasIk)
                {
                    WriteBoneIk(writer, bone, pmxConfig.BoneIndexSize);
                }
            }
        }

        private static void WriteBoneIk(BinaryWriter writer, Bone bone, int boneIndexSize)
        {
            MMDReaderWriteUtil.WriteIndex(writer, bone.IkInfoVal.IkTargetIndex, boneIndexSize);
            writer.Write(bone.IkInfoVal.CcdIterateLimit);
            writer.Write(bone.IkInfoVal.CcdAngleLimit);

            var ikLinkNum = bone.IkInfoVal.IkLinks.Length;
            writer.Write(ikLinkNum);
            for (var j = 0; j < ikLinkNum; ++j)
            {
                var link = bone.IkInfoVal.IkLinks[j];
                MMDReaderWriteUtil.WriteIndex(writer, link.LinkIndex, boneIndexSize);
                writer.Write(link.HasLimit ? (byte)1 : (byte)0);
                if (link.HasLimit)
                {
                    MMDReaderWriteUtil.WriteVector3(writer, link.LoLimit, false);
                    MMDReaderWriteUtil.WriteVector3(writer, link.HiLimit, false);
                }
            }
        }

        private static void WriteParts(BinaryWriter writer, ModelConfig config, RawMMDModel model, PmxConfig pmxConfig)
        {
            var partNum = model.Parts.Length;
            writer.Write(partNum);
            var partBaseShift = 0;
            for (var i = 0; i < partNum; i++)
            {
                var part = model.Parts[i];
                WriteMaterial(writer, part.Material, config, pmxConfig.Encoding, pmxConfig.TextureIndexSize, model.TextureList);
                writer.Write(part.TriangleIndexNum);
                if (part.TriangleIndexNum % 3 != 0)
                {
                    throw new MMDFileParseException($"part{i} triangle index count {part.TriangleIndexNum} is not a multiple of 3");
                }
                partBaseShift += part.TriangleIndexNum;
            }
        }

        private static void WriteMaterial(BinaryWriter writer, MMDMaterial material, ModelConfig config, Encoding encoding,
        int textureIndexSize, List<MMDTexture> textureList)
        {
            MMDReaderWriteUtil.WriteSizedString(writer, material.Name, encoding);
            MMDReaderWriteUtil.WriteSizedString(writer, material.NameEn, encoding);
            MMDReaderWriteUtil.WriteColor(writer, material.DiffuseColor, true);
            MMDReaderWriteUtil.WriteColor(writer, material.SpecularColor, false);
            writer.Write(material.Shiness);
            MMDReaderWriteUtil.WriteColor(writer, material.AmbientColor, false);

            byte drawFlag = 0;
            if (material.DrawDoubleFace)
                drawFlag |= PmxMaterialDrawFlags.PmxMaterialDrawDoubleFace;
            if (material.DrawGroundShadow)
                drawFlag |= PmxMaterialDrawFlags.PmxMaterialDrawGroundShadow;
            if (material.CastSelfShadow)
                drawFlag |= PmxMaterialDrawFlags.PmxMaterialCastSelfShadow;
            if (material.DrawSelfShadow)
                drawFlag |= PmxMaterialDrawFlags.PmxMaterialDrawSelfShadow;
            if (material.DrawEdge)
                drawFlag |= PmxMaterialDrawFlags.PmxMaterialDrawEdge;
            writer.Write(drawFlag);

            MMDReaderWriteUtil.WriteColor(writer, material.EdgeColor, true);
            writer.Write(material.EdgeSize);

            int textureIndex = GetTextureIndex(material.Texture, textureList);
            MMDReaderWriteUtil.WriteIndex(writer, textureIndex, textureIndexSize);

            int subTextureIndex = GetTextureIndex(material.SubTexture, textureList);
            MMDReaderWriteUtil.WriteIndex(writer, subTextureIndex, textureIndexSize);

            writer.Write((byte)material.SubTextureType);

            bool useGlobalToon = false;
            int globalToonIndex = 0;

            if (material.Toon != null)
            {
                if (material.Toon.IsGlobalToon)
                {
                    useGlobalToon = true;
                    globalToonIndex = MMDTextureUtil.GetGlobalToonIndex(material.Toon.TexturePath, config.GlobalToonPath);
                }
            }
            writer.Write((byte)(useGlobalToon ? 1 : 0));



            if (useGlobalToon)
            {
                writer.Write((byte)globalToonIndex);
            }
            else
            {
                int toonIndex = GetTextureIndex(material.Toon, textureList);
                MMDReaderWriteUtil.WriteIndex(writer, toonIndex, textureIndexSize);
            }

            MMDReaderWriteUtil.WriteSizedString(writer, material.MetaInfo, encoding);
        }

        private static int GetTextureIndex(MMDTexture texture, List<MMDTexture> textureList)
        {
            if (texture != null)
            {
                int index = textureList.IndexOf(texture);
                if (index >= 0)
                {
                    return index;
                }
            }
            return -1;
        }

        private static void WriteTextureList(BinaryWriter writer, List<MMDTexture> textureList, PmxConfig pmxConfig)
        {
            var textureNum = textureList.Count;
            writer.Write(textureNum);
            for (var i = 0; i < textureNum; i++)
            {
                var texturePathEncoding = pmxConfig.Utf8Encoding ? Encoding.UTF8 : Encoding.Unicode;
                var texturePath = textureList[i].TexturePath;
                MMDReaderWriteUtil.WriteSizedString(writer, texturePath, texturePathEncoding);
            }
        }

        private static void WriteTriangles(BinaryWriter writer, RawMMDModel model, PmxConfig pmxConfig)
        {
            var triangleIndexCount = model.TriangleIndexes.Length;
            writer.Write(triangleIndexCount);
            if (triangleIndexCount % 3 != 0)
            {
                throw new MMDFileParseException("triangle index count " + triangleIndexCount + " is not a multiple of 3");
            }
            for (var i = 0; i < triangleIndexCount; ++i)
            {
                MMDReaderWriteUtil.WriteIndex(writer, model.TriangleIndexes[i], pmxConfig.VertexIndexSize);
            }
        }

        private static void WriteModelNameAndDescription(BinaryWriter writer, RawMMDModel model, PmxConfig pmxConfig)
        {
            MMDReaderWriteUtil.WriteSizedString(writer, model.Name, pmxConfig.Encoding);
            MMDReaderWriteUtil.WriteSizedString(writer, model.NameEn, pmxConfig.Encoding);
            MMDReaderWriteUtil.WriteSizedString(writer, model.Description, pmxConfig.Encoding);
            MMDReaderWriteUtil.WriteSizedString(writer, model.DescriptionEn, pmxConfig.Encoding);
        }

        private static void WriteVertices(BinaryWriter writer, RawMMDModel model, PmxConfig pmxConfig)
        {
            var vertexNum = model.Vertices.Length;
            writer.Write(vertexNum);
            for (uint i = 0; i < vertexNum; ++i)
            {
                var vertex = model.Vertices[i];
                WriteVertex(writer, vertex, pmxConfig);
            }
        }

        private static void WriteVertex(BinaryWriter writer, Vertex vertex, PmxConfig pmxConfig)
        {
            WriteVertexBasic(writer, vertex.Coordinate, vertex.Normal, vertex.UvCoordinate);

            if (pmxConfig.ExtraUvNumber > 0)
            {
                for (var ei = 0; ei < pmxConfig.ExtraUvNumber; ++ei)
                {
                    MMDReaderWriteUtil.WriteVector4(writer, vertex.ExtraUvCoordinate[ei]);
                }
            }

            var skinningType = (byte)vertex.SkinningOperator.Type;
            writer.Write(skinningType);

            switch ((SkinningOperator.SkinningType)skinningType)
            {
                case SkinningOperator.SkinningType.SkinningBdef1:
                    var bdef1 = (SkinningOperator.Bdef1)vertex.SkinningOperator.Param;
                    MMDReaderWriteUtil.WriteIndex(writer, bdef1.BoneId, pmxConfig.BoneIndexSize);
                    break;
                case SkinningOperator.SkinningType.SkinningBdef2:
                    var bdef2 = (SkinningOperator.Bdef2)vertex.SkinningOperator.Param;
                    MMDReaderWriteUtil.WriteIndex(writer, bdef2.BoneId[0], pmxConfig.BoneIndexSize);
                    MMDReaderWriteUtil.WriteIndex(writer, bdef2.BoneId[1], pmxConfig.BoneIndexSize);
                    writer.Write(bdef2.BoneWeight);
                    break;
                case SkinningOperator.SkinningType.SkinningBdef4:
                    var bdef4 = (SkinningOperator.Bdef4)vertex.SkinningOperator.Param;
                    for (var j = 0; j < 4; ++j)
                    {
                        MMDReaderWriteUtil.WriteIndex(writer, bdef4.BoneId[j], pmxConfig.BoneIndexSize);
                    }
                    for (var j = 0; j < 4; ++j)
                    {
                        writer.Write(bdef4.BoneWeight[j]);
                    }
                    break;
                case SkinningOperator.SkinningType.SkinningSdef:
                    var sdef = (SkinningOperator.Sdef)vertex.SkinningOperator.Param;
                    MMDReaderWriteUtil.WriteIndex(writer, sdef.BoneId[0], pmxConfig.BoneIndexSize);
                    MMDReaderWriteUtil.WriteIndex(writer, sdef.BoneId[1], pmxConfig.BoneIndexSize);
                    writer.Write(sdef.BoneWeight);
                    MMDReaderWriteUtil.WriteVector3(writer, sdef.C);
                    MMDReaderWriteUtil.WriteVector3(writer, sdef.R0);
                    MMDReaderWriteUtil.WriteVector3(writer, sdef.R1);
                    break;
                default:
                    throw new MMDFileParseException("invalid skinning type: " + skinningType);
            }

            writer.Write(vertex.EdgeScale);
        }

        private static void WriteVertexBasic(BinaryWriter writer, Vector3 coordinate, Vector3 normal, Vector2 uvCoordinate)
        {
            MMDReaderWriteUtil.WriteVector3(writer, coordinate);
            MMDReaderWriteUtil.WriteVector3(writer, normal, false);
            MMDReaderWriteUtil.WriteVector2(writer, uvCoordinate);
        }

        private static void WriteMeta(BinaryWriter writer, PmxMeta meta)
        {
            MMDReaderWriteUtil.WriteStringFixedLength(writer, meta.Magic, 4, Encoding.ASCII);
            writer.Write(meta.Version);
            writer.Write(meta.FileFlagSize);
        }

        private static void WritePmxConfig(BinaryWriter writer, PmxConfig pmxConfig)
        {
            writer.Write(pmxConfig.Utf8Encoding ? (byte)1 : (byte)0);
            writer.Write((byte)pmxConfig.ExtraUvNumber);
            writer.Write((byte)pmxConfig.VertexIndexSize);
            writer.Write((byte)pmxConfig.TextureIndexSize);
            writer.Write((byte)pmxConfig.MaterialIndexSize);
            writer.Write((byte)pmxConfig.BoneIndexSize);
            writer.Write((byte)pmxConfig.MorphIndexSize);
            writer.Write((byte)pmxConfig.RigidBodyIndexSize);
        }
    }
}