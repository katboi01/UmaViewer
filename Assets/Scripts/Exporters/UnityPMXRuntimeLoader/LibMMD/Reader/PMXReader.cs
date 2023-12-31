using System;
using System.IO;
using System.Text;
using LibMMD.Material;
using LibMMD.Model;
using LibMMD.Util;
using UnityEngine;

namespace LibMMD.Reader
{
    public class PMXReader : ModelReader
    {
        const float JointSplingAmp = 0.08f;
        const float JointDampAmp = 0.08f;

        public override RawMMDModel Read(BinaryReader reader, ModelConfig config)
        {
            PmxMeta pmxHeader;
            try
            {
                pmxHeader = ReadMeta(reader);
            }
            catch
            {
                throw new MMDFileParseException("Could not read pmx meta data.");
            }

            if (!"PMX ".Equals(pmxHeader.Magic) || Math.Abs(pmxHeader.Version - 2.0f) > 0.0001f || pmxHeader.FileFlagSize !=8)
            {
                throw new MMDFileParseException("File is not a PMX 2.0 file");
            }

            var model = new RawMMDModel();
            var pmxConfig = ReadPmxConfig(reader, model);
            ReadModelNameAndDescription(reader, model, pmxConfig);
            ReadVertices(reader, model, pmxConfig);
            ReadTriangles(reader, model, pmxConfig);
            var textureList = ReadTextureList(reader, pmxConfig);
            model.TextureList.AddRange(textureList);
            ReadParts(reader, config, model, pmxConfig, textureList);
            ReadBones(reader, model, pmxConfig);
            ReadMorphs(reader, model, pmxConfig);
            ReadEntries(reader, pmxConfig);
            ReadRigidBodies(reader, model, pmxConfig);
            ReadJoints(reader, model, pmxConfig);
            model.Normalize();
            return model;
        }

        private static void ReadJoints(BinaryReader reader, RawMMDModel model, PmxConfig pmxConfig)
        {
            var constraintNum = reader.ReadInt32();
            model.Joints = new Model.MMDJoint[constraintNum];
            for (var i = 0; i < constraintNum; ++i)
            {
                var joint = new Model.MMDJoint
                {
                    Name = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding),
                    NameEn = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding)
                };
                var dofType = reader.ReadByte();
                if (dofType == 0)
                {
                    joint.AssociatedRigidBodyIndex[0] =
                        MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.RigidBodyIndexSize);
                    joint.AssociatedRigidBodyIndex[1] =
                        MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.RigidBodyIndexSize);
                    joint.Position = MMDReaderWriteUtil.ReadVector3(reader);
                    joint.Rotation = MMDReaderWriteUtil.ReadAmpVector3(reader, Mathf.Rad2Deg);
                    joint.PositionLowLimit = MMDReaderWriteUtil.ReadVector3(reader);
                    joint.PositionHiLimit = MMDReaderWriteUtil.ReadVector3(reader);
                    joint.RotationLowLimit = MMDReaderWriteUtil.ReadVector3(reader);
                    joint.RotationHiLimit = MMDReaderWriteUtil.ReadVector3(reader);
                    joint.SpringTranslate = MMDReaderWriteUtil.ReadVector3(reader);
                    joint.SpringRotate = MMDReaderWriteUtil.ReadVector3(reader);
                }
                else
                {
                    throw new MMDFileParseException("Only 6DOF spring joints are supported.");
                }

                model.Joints[i] = joint;
            }
        }

        private static PmxConfig ReadPmxConfig(BinaryReader reader, RawMMDModel model)
        {
            var pmxConfig = new PmxConfig();
            pmxConfig.Utf8Encoding = reader.ReadByte() != 0;
            pmxConfig.ExtraUvNumber = reader.ReadSByte();
            pmxConfig.VertexIndexSize = reader.ReadSByte();
            pmxConfig.TextureIndexSize = reader.ReadSByte();
            pmxConfig.MaterialIndexSize = reader.ReadSByte();
            pmxConfig.BoneIndexSize = reader.ReadSByte();
            pmxConfig.MorphIndexSize = reader.ReadSByte();
            pmxConfig.RigidBodyIndexSize = reader.ReadSByte();

            model.ExtraUvNumber = pmxConfig.ExtraUvNumber;
            pmxConfig.Encoding = pmxConfig.Utf8Encoding ? Encoding.UTF8 : Encoding.Unicode;
            return pmxConfig;
        }

        private static void ReadRigidBodies(BinaryReader reader, RawMMDModel model, PmxConfig pmxConfig)
        {
            var rigidBodyNum = reader.ReadInt32();
            model.Rigidbodies = new MMDRigidBody[rigidBodyNum];
            for (var i = 0; i < rigidBodyNum; ++i)
            {
                var rigidBody = new MMDRigidBody
                {
                    Name = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding),
                    NameEn = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding),
                    AssociatedBoneIndex = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize),
                    CollisionGroup = reader.ReadByte(),
                    CollisionMask = reader.ReadUInt16(),
                    Shape = (MMDRigidBody.RigidBodyShape) reader.ReadByte(),
                    Dimemsions = MMDReaderWriteUtil.ReadRawCoordinateVector3(reader),
                    Position = MMDReaderWriteUtil.ReadVector3(reader),
                    Rotation = MMDReaderWriteUtil.ReadAmpVector3(reader, Mathf.Rad2Deg),
                    Mass = reader.ReadSingle(),
                    TranslateDamp = reader.ReadSingle(),
                    RotateDamp = reader.ReadSingle(),
                    Restitution = reader.ReadSingle(),
                    Friction = reader.ReadSingle(),
                    Type = (MMDRigidBody.RigidBodyType) reader.ReadByte()
                };
                model.Rigidbodies[i] = rigidBody;
            }
        }

        //unused data
        private static void ReadEntries(BinaryReader reader, PmxConfig pmxConfig)
        {
            var entryItemNum = reader.ReadInt32();
            for (var i = 0; i < entryItemNum; ++i)
            {
                MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding); //entryItemName
                MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding); //entryItemNameEn
                reader.ReadByte(); //isSpecial
                var elementNum = reader.ReadInt32();
                for (var j = 0; j < elementNum; ++j)
                {
                    var isMorph = reader.ReadByte() == 1;
                    if (isMorph)
                    {
                        MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.MorphIndexSize); //morphIndex
                    }
                    else
                    {
                        MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize); //boneIndex
                    }
                }
            }
        }

        private static void ReadMorphs(BinaryReader reader, RawMMDModel model, PmxConfig pmxConfig)
        {
            var morphNum = reader.ReadInt32();
            int? baseMorphIndex = null;
            model.Morphs = new Morph[morphNum];
            for (var i = 0; i < morphNum; ++i)
            {
                var morph = new Morph
                {
                    Name = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding),
                    NameEn = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding),
                    Category = (Morph.MorphCategory) reader.ReadByte()
                };
                if (morph.Category == Morph.MorphCategory.MorphCatSystem)
                {
                    baseMorphIndex = i;
                }
                morph.Type = (Morph.MorphType) reader.ReadByte();
                var morphDataNum = reader.ReadInt32();
                morph.MorphDatas = new Morph.MorphData[morphDataNum];
                switch (morph.Type)
                {
                    case Morph.MorphType.MorphTypeGroup:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.GroupMorphData
                                {
                                    MorphIndex = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.MorphIndexSize),
                                    MorphRate = reader.ReadSingle()
                                };
                            morph.MorphDatas[j] = morphData;
                        }
                        break;
                    case Morph.MorphType.MorphTypeVertex:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.VertexMorphData
                                {
                                    VertexIndex = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.VertexIndexSize),
                                    Offset = MMDReaderWriteUtil.ReadVector3(reader)
                                };
                            morph.MorphDatas[j] = morphData;
                        }
                        break;
                    case Morph.MorphType.MorphTypeBone:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.BoneMorphData
                                {
                                    BoneIndex = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize),
                                    Translation = MMDReaderWriteUtil.ReadVector3(reader),
                                    Rotation = MMDReaderWriteUtil.ReadQuaternion(reader)
                                };
                            morph.MorphDatas[j] = morphData;
                        }

                        break;
                    case Morph.MorphType.MorphTypeUv:
                    case Morph.MorphType.MorphTypeExtUv1:
                    case Morph.MorphType.MorphTypeExtUv2:
                    case Morph.MorphType.MorphTypeExtUv3:
                    case Morph.MorphType.MorphTypeExtUv4:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.UvMorphData
                                {
                                    VertexIndex = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.VertexIndexSize),
                                    Offset = MMDReaderWriteUtil.ReadVector4(reader)
                                };
                            morph.MorphDatas[j] = morphData;
                        }

                        break;
                    case Morph.MorphType.MorphTypeMaterial:
                        for (var j = 0; j < morphDataNum; j++)
                        {
                            var morphData = new Morph.MaterialMorphData();
                            var mmIndex = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.MaterialIndexSize);
                            if (mmIndex < model.Parts.Length && mmIndex > 0) //TODO mmdlib的代码里是和bone数比较。确认这个逻辑
                            {
                                morphData.MaterialIndex = mmIndex;
                                morphData.Global = false;
                            }
                            else
                            {
                                morphData.MaterialIndex = 0;
                                morphData.Global = true;
                            }
                            morphData.Method = (Morph.MaterialMorphData.MaterialMorphMethod) reader.ReadByte();
                            morphData.Diffuse = MMDReaderWriteUtil.ReadColor(reader, true);
                            morphData.Specular = MMDReaderWriteUtil.ReadColor(reader, false);
                            morphData.Shiness = reader.ReadSingle();
                            morphData.Ambient = MMDReaderWriteUtil.ReadColor(reader, false);
                            morphData.EdgeColor = MMDReaderWriteUtil.ReadColor(reader, true);
                            morphData.EdgeSize = reader.ReadSingle();
                            morphData.Texture = MMDReaderWriteUtil.ReadVector4(reader);
                            morphData.SubTexture = MMDReaderWriteUtil.ReadVector4(reader);
                            morphData.ToonTexture = MMDReaderWriteUtil.ReadVector4(reader);
                            morph.MorphDatas[j] = morphData;
                        }
                        break;
                    default:
                        throw new MMDFileParseException("invalid morph type " + morph.Type);
                }
                if (baseMorphIndex != null)
                {
                    //TODO rectify system-reserved category
                }

                model.Morphs[i] = morph;
            }
        }

        private static void ReadBones(BinaryReader reader, RawMMDModel model, PmxConfig pmxConfig)
        {
            var boneNum = reader.ReadInt32();
            model.Bones = new Bone[boneNum];
            for (var i = 0; i < boneNum; ++i)
            {
                var bone = new Bone
                {
                    Name = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding),
                    NameEn = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding),
                    Position = MMDReaderWriteUtil.ReadVector3(reader)
                };
                var parentIndex = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                if (parentIndex < boneNum && parentIndex >= 0)
                {
                    bone.ParentIndex = parentIndex;
                }
                else
                {
                    bone.ParentIndex = -1;
                }
                bone.TransformLevel = reader.ReadInt32();
                var flag = reader.ReadUInt16();
                bone.ChildBoneVal.ChildUseId = (flag & PmxBoneFlags.PmxBoneChildUseId) != 0;
                bone.Rotatable = (flag & PmxBoneFlags.PmxBoneRotatable) != 0;
                bone.Movable = (flag & PmxBoneFlags.PmxBoneMovable) != 0;
                bone.Visible = (flag & PmxBoneFlags.PmxBoneVisible) != 0;
                bone.Controllable = (flag & PmxBoneFlags.PmxBoneControllable) != 0;
                bone.HasIk = (flag & PmxBoneFlags.PmxBoneHasIk) != 0;
                bone.AppendRotate = (flag & PmxBoneFlags.PmxBoneAcquireRotate) != 0;
                bone.AppendTranslate = (flag & PmxBoneFlags.PmxBoneAcquireTranslate) != 0;
                bone.RotAxisFixed = (flag & PmxBoneFlags.PmxBoneRotAxisFixed) != 0;
                bone.UseLocalAxis = (flag & PmxBoneFlags.PmxBoneUseLocalAxis) != 0;
                bone.PostPhysics = (flag & PmxBoneFlags.PmxBonePostPhysics) != 0;
                bone.ReceiveTransform = (flag & PmxBoneFlags.PmxBoneReceiveTransform) != 0;
                if (bone.ChildBoneVal.ChildUseId)
                {
                    bone.ChildBoneVal.Index = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                }
                else
                {
                    bone.ChildBoneVal.Offset = MMDReaderWriteUtil.ReadVector3(reader);
                }
                if (bone.RotAxisFixed)
                {
                    bone.RotAxis = MMDReaderWriteUtil.ReadVector3(reader);
                }
                if (bone.AppendRotate || bone.AppendTranslate)
                {
                    bone.AppendBoneVal.Index = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    bone.AppendBoneVal.Ratio = reader.ReadSingle();
                }
                if (bone.UseLocalAxis)
                {
                    var localX = MMDReaderWriteUtil.ReadVector3(reader);
                    var localZ = MMDReaderWriteUtil.ReadVector3(reader);
                    var localY = Vector3.Cross(localX, localZ);
                    localZ = Vector3.Cross(localX, localY);
                    localX.Normalize();
                    localY.Normalize();
                    localZ.Normalize();
                    bone.LocalAxisVal.AxisX = localX;
                    bone.LocalAxisVal.AxisY = localY;
                    bone.LocalAxisVal.AxisZ = localZ;
                }
                if (bone.ReceiveTransform)
                {
                    bone.ExportKey = reader.ReadInt32();
                }
                if (bone.HasIk)
                {
                    ReadBoneIk(reader, bone, pmxConfig.BoneIndexSize);
                }

                model.Bones[i] = bone;
            }
        }

        private static void ReadBoneIk(BinaryReader reader, Bone bone, int boneIndexSize)
        {
            bone.IkInfoVal = new Bone.IkInfo();
            bone.IkInfoVal.IkTargetIndex = MMDReaderWriteUtil.ReadIndex(reader, boneIndexSize);
            bone.IkInfoVal.CcdIterateLimit = reader.ReadInt32();
            bone.IkInfoVal.CcdAngleLimit = reader.ReadSingle();
            var ikLinkNum = reader.ReadInt32();
            bone.IkInfoVal.IkLinks = new Bone.IkLink[ikLinkNum];
            for (var j = 0; j < ikLinkNum; ++j)
            {
                var link = new Bone.IkLink();
                link.LinkIndex = MMDReaderWriteUtil.ReadIndex(reader, boneIndexSize);
                link.HasLimit = reader.ReadByte() != 0;
                if (link.HasLimit)
                {
                    link.LoLimit = MMDReaderWriteUtil.ReadVector3(reader);
                    link.HiLimit = MMDReaderWriteUtil.ReadVector3(reader);
                }
                bone.IkInfoVal.IkLinks[j] = link;
            }
        }

        private static void ReadParts(BinaryReader reader, ModelConfig config, RawMMDModel model, PmxConfig pmxConfig, MMDTexture[] textureList)
        {
            var partNum = reader.ReadInt32();
            var partBaseShift = 0;
            model.Parts = new Part[partNum];
            for (var i = 0; i < partNum; i++)
            {
                var part = new Part();
                var material = ReadMaterial(reader, config, pmxConfig.Encoding, pmxConfig.TextureIndexSize, textureList);
                part.Material = material;
                var partTriangleIndexNum = reader.ReadInt32();
                if (partTriangleIndexNum % 3 != 0)
                {
                    throw new MMDFileParseException("part" + i + " triangle index count " + partTriangleIndexNum +
                                                    " is not multiple of 3");
                }
                part.BaseShift = partBaseShift;
                part.TriangleIndexNum = partTriangleIndexNum;
                partBaseShift += partTriangleIndexNum;
                model.Parts[i] = part;
            }
        }

        private static MMDTexture[] ReadTextureList(BinaryReader reader, PmxConfig pmxConfig)
        {
            var textureNum = reader.ReadInt32();
            var textureList = new MMDTexture[textureNum];
            for (var i = 0; i < textureNum; ++i)
            {
                var texturePathEncoding = pmxConfig.Utf8Encoding ? Encoding.UTF8 : Encoding.Unicode;
                var texturePath = MMDReaderWriteUtil.ReadSizedString(reader, texturePathEncoding);
                textureList[i] = new MMDTexture(texturePath);
            }
            return textureList;
        }

        private static void ReadTriangles(BinaryReader reader, RawMMDModel model, PmxConfig pmxConfig)
        {
            var triangleIndexCount = reader.ReadInt32();
            model.TriangleIndexes = new int[triangleIndexCount];
            if (triangleIndexCount % 3 != 0)
            {
                throw new MMDFileParseException("triangle index count " + triangleIndexCount + " is not multiple of 3");
            }
            for (var i = 0; i < triangleIndexCount; ++i)
            {
                model.TriangleIndexes[i] = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.VertexIndexSize);
            }
        }

        private static void ReadVertices(BinaryReader reader, RawMMDModel model, PmxConfig pmxConfig)
        {
            var vertexNum = reader.ReadInt32();
            model.Vertices = new Vertex[vertexNum];
            for (uint i = 0; i < vertexNum; ++i)
            {
                var vertex = ReadVertex(reader, pmxConfig);
                model.Vertices[i] = vertex;
            }
        }

        private static void ReadModelNameAndDescription(BinaryReader reader, RawMMDModel model, PmxConfig pmxConfig)
        {
            model.Name = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding);
            model.NameEn = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding);
            model.Description = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding);
            model.DescriptionEn = MMDReaderWriteUtil.ReadSizedString(reader, pmxConfig.Encoding);
        }

        private static Vertex ReadVertex(BinaryReader reader, PmxConfig pmxConfig)
        {
            var pv = ReadVertexBasic(reader);
            var vertex = new Vertex
            {
                Coordinate = pv.Coordinate,
                Normal = pv.Normal,
                UvCoordinate = pv.UvCoordinate
            };

            if (pmxConfig.ExtraUvNumber > 0)
            {
                var extraUv = new Vector4[pmxConfig.ExtraUvNumber];
                for (var ei = 0; ei < pmxConfig.ExtraUvNumber; ++ei)
                {
                    extraUv[ei] = MMDReaderWriteUtil.ReadVector4(reader);
                }
                vertex.ExtraUvCoordinate = extraUv;
            }

            var op = new SkinningOperator();
            var skinningType = (SkinningOperator.SkinningType) reader.ReadByte();
            op.Type = skinningType;

            switch (skinningType)
            {
                case SkinningOperator.SkinningType.SkinningBdef1:
                    var bdef1 = new SkinningOperator.Bdef1();
                    bdef1.BoneId = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    op.Param = bdef1;
                    break;
                case SkinningOperator.SkinningType.SkinningBdef2:
                    var bdef2 = new SkinningOperator.Bdef2();
                    bdef2.BoneId[0] = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    bdef2.BoneId[1] = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    bdef2.BoneWeight = reader.ReadSingle();
                    op.Param = bdef2;
                    break;
                case SkinningOperator.SkinningType.SkinningBdef4:
                    var bdef4 = new SkinningOperator.Bdef4();
                    for (var j = 0; j < 4; ++j)
                    {
                        bdef4.BoneId[j] = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    }
                    for (var j = 0; j < 4; ++j)
                    {
                        bdef4.BoneWeight[j] = reader.ReadSingle();
                    }
                    op.Param = bdef4;
                    break;
                case SkinningOperator.SkinningType.SkinningSdef:
                    var sdef = new SkinningOperator.Sdef();
                    sdef.BoneId[0] = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    sdef.BoneId[1] = MMDReaderWriteUtil.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    sdef.BoneWeight = reader.ReadSingle();
                    sdef.C = MMDReaderWriteUtil.ReadVector3(reader);
                    sdef.R0 = MMDReaderWriteUtil.ReadVector3(reader);
                    sdef.R1 = MMDReaderWriteUtil.ReadVector3(reader);
                    op.Param = sdef;
                    break;
                default:
                    throw new MMDFileParseException("invalid skinning type: " + skinningType);
            }
            vertex.SkinningOperator = op;
            vertex.EdgeScale = reader.ReadSingle();
            return vertex;
        }

        private static MMDMaterial ReadMaterial(BinaryReader reader, ModelConfig config, Encoding encoding,
            int textureIndexSize, MMDTexture[] textureList)
        {
            var material = new MMDMaterial();
            material.Name = MMDReaderWriteUtil.ReadSizedString(reader, encoding);
            material.NameEn = MMDReaderWriteUtil.ReadSizedString(reader, encoding);
            material.DiffuseColor = MMDReaderWriteUtil.ReadColor(reader, true);
            material.SpecularColor = MMDReaderWriteUtil.ReadColor(reader, false);
            material.Shiness = reader.ReadSingle();
            material.AmbientColor = MMDReaderWriteUtil.ReadColor(reader, false);
            var drawFlag = reader.ReadByte();
            material.DrawDoubleFace = (drawFlag & PmxMaterialDrawFlags.PmxMaterialDrawDoubleFace) != 0;
            material.DrawGroundShadow = (drawFlag & PmxMaterialDrawFlags.PmxMaterialDrawGroundShadow) != 0;
            material.CastSelfShadow = (drawFlag & PmxMaterialDrawFlags.PmxMaterialCastSelfShadow) != 0;
            material.DrawSelfShadow = (drawFlag & PmxMaterialDrawFlags.PmxMaterialDrawSelfShadow) != 0;
            material.DrawEdge = (drawFlag & PmxMaterialDrawFlags.PmxMaterialDrawEdge) != 0;
            material.EdgeColor = MMDReaderWriteUtil.ReadColor(reader, true);
            material.EdgeSize = reader.ReadSingle();
            var textureIndex = MMDReaderWriteUtil.ReadIndex(reader, textureIndexSize);
            if (textureIndex < textureList.Length && textureIndex >= 0)
            {
                material.Texture = textureList[textureIndex];
            }
            var subTextureIndex = MMDReaderWriteUtil.ReadIndex(reader, textureIndexSize);
            if (subTextureIndex < textureList.Length && subTextureIndex >= 0)
            {
                material.SubTexture = textureList[subTextureIndex];
            }
            material.SubTextureType = (MMDMaterial.SubTextureTypeEnum) reader.ReadByte();
            var useGlobalToon = reader.ReadByte() != 0;
            if (useGlobalToon)
            {
                int globalToonIndex = reader.ReadByte();
                material.Toon = MMDTextureUtil.GetGlobalToon(globalToonIndex);
            }
            else
            {
                var toonIndex = MMDReaderWriteUtil.ReadIndex(reader, textureIndexSize);
                if (toonIndex < textureList.Length && toonIndex >= 0)
                {
                    material.Toon = textureList[toonIndex];
                }
            }
            material.MetaInfo = MMDReaderWriteUtil.ReadSizedString(reader, encoding);
            return material;
        }

        private static PmxMeta ReadMeta(BinaryReader reader)
        {
            PmxMeta ret;
            ret.Magic = MMDReaderWriteUtil.ReadStringFixedLength(reader, 4, Encoding.ASCII);
            ret.Version = reader.ReadSingle();
            ret.FileFlagSize = reader.ReadByte();
            return ret;
        }

        private static PmxVertexBasic ReadVertexBasic(BinaryReader reader)
        {
            PmxVertexBasic ret;
            ret.Coordinate = MMDReaderWriteUtil.ReadVector3(reader);
            ret.Normal = MMDReaderWriteUtil.ReadVector3(reader);
            ret.UvCoordinate = MMDReaderWriteUtil.ReadVector2(reader);
            return ret;
        }


        public struct PmxMeta
        {
            public string Magic;
            public float Version;
            public byte FileFlagSize;
        }

        public struct PmxVertexBasic
        {
            public Vector3 Coordinate;
            public Vector3 Normal;
            public Vector2 UvCoordinate;
        }

        public class PmxConfig
        {
            public bool Utf8Encoding { get; set; }
            public Encoding Encoding { get; set; }
            public int ExtraUvNumber { get; set; }
            public int VertexIndexSize { get; set; }
            public int TextureIndexSize { get; set; }
            public int MaterialIndexSize{ get; set; }
            public int BoneIndexSize { get; set; }
            public int MorphIndexSize  { get; set; }
            public int RigidBodyIndexSize { get; set; }
        }

        public abstract class PmxMaterialDrawFlags
        {
            public const byte PmxMaterialDrawDoubleFace = 0x01;
            public const byte PmxMaterialDrawGroundShadow = 0x02;
            public const byte PmxMaterialCastSelfShadow = 0x04;
            public const byte PmxMaterialDrawSelfShadow = 0x08;
            public const byte PmxMaterialDrawEdge = 0x10;
        }

        public abstract class PmxBoneFlags
        {
            public const ushort PmxBoneChildUseId = 0x0001;
            public const ushort PmxBoneRotatable = 0x0002;
            public const ushort PmxBoneMovable = 0x0004;
            public const ushort PmxBoneVisible = 0x0008;
            public const ushort PmxBoneControllable = 0x0010;
            public const ushort PmxBoneHasIk = 0x0020;
            public const ushort PmxBoneAcquireRotate = 0x0100;
            public const ushort PmxBoneAcquireTranslate = 0x0200;
            public const ushort PmxBoneRotAxisFixed = 0x0400;
            public const ushort PmxBoneUseLocalAxis = 0x0800;
            public const ushort PmxBonePostPhysics = 0x1000;
            public const ushort PmxBoneReceiveTransform = 0x2000;
        }
    }
}