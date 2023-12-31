using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibMMD.Model;
using LibMMD.Reader;
using UnityEngine;
using System.Threading.Tasks;

namespace LibMMD.Unity3D
{
    public class MMDModel : MonoBehaviour
    {
        public static Dictionary<string, string> Unity_MMDBoneNameDictionary = new Dictionary<string, string>()
        {
            {"Hips","センター"},
            {"LeftUpperLeg","左足"},
            {"RightUpperLeg","右足"},
            {"LeftLowerLeg","左ひざ"},
            {"RightLowerLeg","右ひざ"},
            {"LeftFoot","左足首"},
            {"RightFoot","右足首"},
            {"Spine","上半身"},
            {"Chest", null},
            {"Neck","首"},
            {"Head","頭"},
            { LeftShoulder,"左肩"},
            { RightShoulder,"右肩"},
            { LeftUpperArm,"左腕"},
            { RightUpperArm,"右腕"},
            { LeftLowerArm,"左ひじ"},
            { RightLowerArm,"右ひじ"},
            { LeftHand,"左手首"},
            { RightHand,"右手首"},
            {"LeftToes","左つま先"},
            {"RightToes","右つま先"},
            {"LeftEye","左目"},
            {"RightEye","右目"},
            {"Jaw", null},
            { LeftThumbProximal, "左親指０"},
            { LeftThumbIntermediate,"左親指１"},
            { LeftThumbDistal,"左親指２"},
            { LeftIndexProximal,"左人指１"},
            {"Left Index Intermediate","左人指２"},
            { LeftIndexDistal,"左人指３"},
            { LeftMiddleProximal,"左中指１"},
            {"Left Middle Intermediate","左中指２"},
            { LeftMiddleDistal,"左中指３"},
            { LeftRingProximal,"左薬指１"},
            {"Left Ring Intermediate","左薬指２"},
            { LeftRingDistal,"左薬指３"},
            { LeftLittleProximal,"左小指１"},
            {"Left Little Intermediate","左小指２"},
            { LeftLittleDistal,"左小指３"},
            { RightThumbProximal, "右親指０"},
            { RightThumbIntermediate,"右親指１"},
            { RightThumbDistal,"右親指２"},
            { RightIndexProximal,"右人指１"},
            {"Right Index Intermediate","右人指２"},
            { RightIndexDistal,"右人指３"},
            { RightMiddleProximal,"右中指１"},
            {"Right Middle Intermediate","右中指２"},
            { RightMiddleDistal,"右中指３"},
            { RightRingProximal,"右薬指１"},
            {"Right Ring Intermediate","右薬指２"},
            { RightRingDistal,"右薬指３"},
            { RightLittleProximal,"右小指１"},
            {"Right Little Intermediate","右小指２"},
            { RightLittleDistal,"右小指３"},
            {"UpperChest", "上半身2"}
        };
        public static Dictionary<string, string> Alt_OriginalNameDictionary = new Dictionary<string, string>()
        {
            {"右足先EX", "右つま先"},
            {"左足先EX", "左つま先"},
            {"右ひじ袖", "右ひじ"},
            {"左ひじ袖", "左ひじ"}
        };
        public const string LeftShoulder = "LeftShoulder";
        public const string RightShoulder = "RightShoulder";
        public const string LeftUpperArm = "LeftUpperArm";
        public const string RightUpperArm = "RightUpperArm";
        public const string LeftLowerArm = "LeftLowerArm";
        public const string RightLowerArm = "RightLowerArm";
        public const string LeftHand = "LeftHand";
        public const string RightHand = "RightHand";
        public const string RightIndexProximal = "Right Index Proximal";
        public const string RightIndexIntermediate = "Right Index Intermediate";
        public const string RightIndexDistal = "Right Index Distal";
        public const string LeftIndexProximal = "Left Index Proximal";
        public const string LeftIndexIntermediate = "Left Index Intermediate";
        public const string LeftIndexDistal = "Left Index Distal";
        public const string RightMiddleProximal = "Right Middle Proximal";
        public const string RightMiddleIntermediate = "Right Middle Intermediate";
        public const string RightMiddleDistal = "Right Middle Distal";
        public const string LeftMiddleProximal = "Left Middle Proximal";
        public const string LeftMiddleIntermediate = "Left Middle Intermediate";
        public const string LeftMiddleDistal = "Left Middle Distal";
        public const string RightRingProximal = "Right Ring Proximal";
        public const string RightRingIntermediate = "Right Ring Intermediate";
        public const string RightRingDistal = "Right Ring Distal";
        public const string LeftRingProximal = "Left Ring Proximal";
        public const string LeftRingIntermediate = "Left Ring Intermediate";
        public const string LeftRingDistal = "Left Ring Distal";
        public const string RightLittleProximal = "Right Little Proximal";
        public const string RightLittleIntermediate = "Right Little Intermediate";
        public const string RightLittleDistal = "Right Little Distal";
        public const string LeftLittleProximal = "Left Little Proximal";
        public const string LeftLittleIntermediate = "Left Little Intermediate";
        public const string LeftLittleDistal = "Left Little Distal";
        public const string RightThumbProximal = "Right Thumb Proximal";
        public const string RightThumbIntermediate = "Right Thumb Intermediate";
        public const string RightThumbDistal = "Right Thumb Distal";
        public const string LeftThumbProximal = "Left Thumb Proximal";
        public const string LeftThumbIntermediate = "Left Thumb Intermediate";
        public const string LeftThumbDistal = "Left Thumb Distal";

        public const string BoneDAppend = "D";
        public const string BoneSAppend = "S";
        public const string BoneEXAppend = "EX";

        //can be null
        public Transform ModelRootTransform { get; private set; }

        public string ModelPath { get; private set; } = "ModelPath";
        public RawMMDModel RawMMDModel { get; private set; }
        public SkinnedMeshRenderer SkinnedMeshRenderer { get; private set; } = new SkinnedMeshRenderer();
        public Mesh Mesh { get; private set; }
        public UnityEngine.Material[] Materials { get; private set; } = new UnityEngine.Material[] { };
        public List<string> MaterialMorphNames { get; private set; } = new List<string>();

        public Dictionary<Transform, int> OriginalBone_IndexDictionary { get; private set; }
            = new Dictionary<Transform, int>();
        public Dictionary<Transform, Transform> Alt_OriginalBoneDictionary { get; private set; }
            = new Dictionary<Transform, Transform>();


        private const string ColliderNameTail = "Collider";
        private const float HalfPIDeg = Mathf.PI * Mathf.Rad2Deg / 2;
        private const int MaxTextureSize = 1024;
        private readonly ModelConfig modelReadConfig = new ModelConfig { GlobalToonPath = "" };

        public static async Task<MMDModel> ImportModel(string filePath, bool autoShowModel = true)
        {
            GameObject modelObject = new GameObject();
            MMDModel mmdModel = modelObject.AddComponent<MMDModel>();
            await mmdModel.LoadModel(filePath);
            if (autoShowModel) { mmdModel.ShowModel(); }
            return mmdModel;
        }

        async Task<bool> LoadModel(string filePath)
        {
            ModelPath = filePath;
            try
            {
                DirectoryInfo directoryInfo = new FileInfo(filePath).Directory;
                if (directoryInfo == null)
                {
                    throw new MMDFileParseException(filePath + " does not belong to any directory.");
                }

                RawMMDModel = ModelReader.LoadMmdModel(filePath, modelReadConfig);
                transform.name = RawMMDModel.Name;
                string relativePath = directoryInfo.FullName;
                MaterialLoader materialLoader = new MaterialLoader(new TextureLoader(relativePath, MaxTextureSize));
                int vertCount = RawMMDModel.Vertices.Length;
                int triangleCount = RawMMDModel.TriangleIndexes.Length / 3;
                int[] triangles = RawMMDModel.TriangleIndexes;
                string meshName = Path.GetFileName(filePath);
                Mesh mesh = new Mesh
                {
                    name = meshName,
                    vertices = new Vector3[vertCount],
                    normals = new Vector3[vertCount]
                };
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                FillSubMesh(mesh, triangleCount, triangles);
                float[] uv = ExtratUv(RawMMDModel.Vertices);
                mesh.uv = Utils.MmdUvToUnityUv(uv);
                mesh.vertices = RawMMDModel.Vertices.Select(x => x.Coordinate).ToArray();
                mesh.normals = RawMMDModel.Vertices.Select(x => x.Normal).ToArray();
                mesh.boneWeights = RawMMDModel.Vertices.Select(x => ConvertBoneWeight(x.SkinningOperator)).ToArray();
                mesh.RecalculateBounds();
                Mesh = mesh;

                await AttachVertexMorph(mesh);
                await SearchForMaterialMorph();

                Utils.ClearAllTransformChild(transform);
                (GameObject rootObj, Transform[] bones) = CreateBones(model: RawMMDModel, parentOfAll: gameObject);

                SkinnedMeshRenderer = rootObj.AddComponent<SkinnedMeshRenderer>();
                SkinnedMeshRenderer.enabled = false;
                SkinnedMeshRenderer.quality = SkinQuality.Bone4;
                SkinnedMeshRenderer.rootBone = rootObj.transform;

                InitializeBoneDictionaries(bones);

                Materials = LoadModelMaterials(materialLoader, new MMDUnityConfig(), RawMMDModel);
                ReorderRender(Materials);

                SkinnedMeshRenderer.sharedMesh = mesh;
                BuildBindpose(Mesh, bones, SkinnedMeshRenderer);
                SkinnedMeshRenderer.materials = Materials;

                MaterialMorphRemover.HideMaterialMorphs(this);

                AttachPhysicComponents(bones, this);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }
            return true;
        }

        private static float[] ExtratUv(Vertex[] verts)
        {
            var ret = new float[verts.Length * 2];
            for (var i = 0; i < verts.Length; ++i)
            {
                ret[2 * i] = verts[i].UvCoordinate.x;
                ret[2 * i + 1] = verts[i].UvCoordinate.y;
            }
            return ret;
        }

        public UnityEngine.Material[] LoadModelMaterials(MaterialLoader materialLoader, MMDUnityConfig config, RawMMDModel model)
        {
            var ret = new UnityEngine.Material[model.Parts.Length];
            for (var i = 0; i < model.Parts.Length; i++)
            {
                ret[i] = materialLoader.LoadMaterial(model.Parts[i].Material, config);
                ret[i].name = model.Parts[i].Material.Name;
            }
            return ret;
        }

        private void FillSubMesh(Mesh mesh, int triangleCount, int[] triangles)
        {
            mesh.subMeshCount = RawMMDModel.Parts.Length;
            for (var i = 0; i < RawMMDModel.Parts.Length; i++)
            {
                var modelPart = RawMMDModel.Parts[i];
                if ((modelPart.BaseShift + modelPart.TriangleIndexNum) / 3 > triangleCount)
                {
                    UnityEngine.Debug.LogError("too many triangles in model part " + i);
                    continue;
                }
                mesh.SetTriangles(Utils.ArrayToList(triangles, modelPart.BaseShift, modelPart.TriangleIndexNum), i);
            }
        }

        private void InitializeBoneDictionaries(Transform[] bones)
        {
            int boneCount = bones.Length;
            for (int i = 0; i < boneCount; i++)
            {
                if (Unity_MMDBoneNameDictionary.Values.Contains(bones[i].name))
                {
                    OriginalBone_IndexDictionary.Add(bones[i], i);
                }
            }

            foreach (Transform bone in bones)
            {
                if (OriginalBone_IndexDictionary.ContainsKey(bone)) { continue; }
                if (Alt_OriginalBoneDictionary.ContainsKey(bone)) { continue; }

                foreach (Transform originalBone in OriginalBone_IndexDictionary.Keys)
                {
                    if (bone.name.Equals(originalBone.name + BoneDAppend)
                        || bone.name.Equals(originalBone.name + BoneSAppend)
                        || bone.name.Equals(originalBone.name + BoneEXAppend))
                    {
                        Alt_OriginalBoneDictionary.Add(bone, originalBone);
                    }
                }
            }
        }

        private void AttachPhysicComponents(Transform[] boneTransforms, MMDModel model)
        {
            GameObject[] bones = boneTransforms.Select(x => x.gameObject).ToArray();
            int bonesCount = bones.Length;
            Transform parentOfAll = model.gameObject.transform;
            Dictionary<int, List<Collider>> colliderGroups = new Dictionary<int, List<Collider>>();
            int rigidbodiesCount = model.RawMMDModel.Rigidbodies.Length;
            for (int i = 0; i < rigidbodiesCount; i++)
            {
                MMDRigidBody mmdRigidBody = model.RawMMDModel.Rigidbodies[i];
                if (bonesCount <= mmdRigidBody.AssociatedBoneIndex) { continue; }
                GameObject bone = bones[mmdRigidBody.AssociatedBoneIndex];
                if (Alt_OriginalBoneDictionary.ContainsKey(bone.transform))
                {
                    bone = Alt_OriginalBoneDictionary[bone.transform].gameObject;
                }

                if (mmdRigidBody.Type == MMDRigidBody.RigidBodyType.RigidTypePhysicsGhost
                    || mmdRigidBody.Type == MMDRigidBody.RigidBodyType.RigidTypePhysicsStrict)
                {
                    continue;
                }

                if (!colliderGroups.ContainsKey(mmdRigidBody.CollisionGroup))
                {
                    colliderGroups.Add(mmdRigidBody.CollisionGroup, new List<Collider>());
                }

                GameObject colliderObject = new GameObject(mmdRigidBody.Name + ColliderNameTail);
                colliderObject.transform.parent = bone.transform;
                Vector3 bonePosition = bone.transform.position - parentOfAll.position;
                colliderObject.transform.localPosition = mmdRigidBody.Position - bonePosition;
                colliderObject.transform.localRotation = Quaternion.Euler(mmdRigidBody.Rotation);
                switch (mmdRigidBody.Shape)
                {
                    case MMDRigidBody.RigidBodyShape.RigidShapeBox:
                        BoxCollider boxCollider = colliderObject.AddComponent<BoxCollider>();
                        colliderGroups[mmdRigidBody.CollisionGroup].Add(boxCollider);
                        boxCollider.size = mmdRigidBody.Dimemsions;
                        break;
                    case MMDRigidBody.RigidBodyShape.RigidShapeCapsule:
                        CapsuleCollider capsuleCollider = colliderObject.AddComponent<CapsuleCollider>();
                        colliderGroups[mmdRigidBody.CollisionGroup].Add(capsuleCollider);
                        capsuleCollider.radius = mmdRigidBody.Dimemsions.x;
                        capsuleCollider.height = mmdRigidBody.Dimemsions.y;
                        break;
                    case MMDRigidBody.RigidBodyShape.RigidShapeSphere:
                        SphereCollider sphereCollider = colliderObject.AddComponent<SphereCollider>();
                        colliderGroups[mmdRigidBody.CollisionGroup].Add(sphereCollider);
                        sphereCollider.radius = mmdRigidBody.Dimemsions.x;
                        break;
                }

                if (bone.GetComponent<Rigidbody>() != null) { continue; }
                Rigidbody rigidbody = bone.AddComponent<Rigidbody>();
                rigidbody.useGravity = true;
                rigidbody.mass = mmdRigidBody.Mass;
                rigidbody.drag = mmdRigidBody.TranslateDamp;
                rigidbody.angularDrag = mmdRigidBody.RotateDamp;
                if (mmdRigidBody.Type == MMDRigidBody.RigidBodyType.RigidTypeKinematic)
                {
                    rigidbody.isKinematic = true;
                }
            }

            //以下ジョイント
            int jointCount = model.RawMMDModel.Joints.Length;
            for (int i = 0; i < jointCount; i++)
            {
                LibMMD.Model.MMDJoint mmdJoint = model.RawMMDModel.Joints[i];
                int jointFromBoneIndex = model.RawMMDModel.Rigidbodies[mmdJoint.AssociatedRigidBodyIndex[0]].AssociatedBoneIndex;
                int jointToBoneIndex = model.RawMMDModel.Rigidbodies[mmdJoint.AssociatedRigidBodyIndex[1]].AssociatedBoneIndex;
                if (bonesCount <= jointFromBoneIndex || bonesCount <= jointToBoneIndex) { continue; }
                GameObject jointFromObj = bones[jointFromBoneIndex];
                GameObject jointToObj = bones[jointToBoneIndex];
                Rigidbody jointToRigidbody = jointToObj.GetComponent<Rigidbody>();
                if (jointToRigidbody == null) { continue; }
                Rigidbody jointFromRigidbody = jointFromObj.GetComponent<Rigidbody>();
                if (jointFromRigidbody == null)
                {
                    //この場合jointFromObjは基本的に体の一部などの始点
                    jointFromRigidbody = jointFromObj.AddComponent<Rigidbody>();
                    jointFromRigidbody.isKinematic = true;
                }
                //すでに向こう側から接続していたら接続しない
                HingeJoint[] counterJoint = jointToObj.GetComponents<HingeJoint>();
                if (counterJoint.Any(x => x != null && jointFromRigidbody != null && x.connectedBody == jointFromRigidbody)) { continue; }
                //残念ながら情報を落とす
                HingeJoint joint = jointFromObj.AddComponent<HingeJoint>();
                joint.connectedBody = jointToRigidbody;
                joint.axis = Quaternion.Euler(mmdJoint.Rotation) * jointFromObj.transform.right;
                joint.limits = new JointLimits
                {
                    max = HalfPIDeg,
                    min = -HalfPIDeg
                };
                joint.useLimits = true;
                joint.spring = new JointSpring
                {
                    spring = mmdJoint.SpringTranslate.magnitude
                };
                joint.useSpring = true;
            }

            foreach (List<Collider> colliderGroup in colliderGroups.Values)
            {
                int colliderCount = colliderGroup.Count;
                for (int i = 0; i < colliderCount; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        Physics.IgnoreCollision(colliderGroup[i], colliderGroup[j]);
                    }
                }
            }
        }

        private async Task AttachVertexMorph(Mesh mesh)
        {
            List<Morph> vertexMorphs = new List<Morph>();
            foreach (Morph morph in RawMMDModel.Morphs)
            {
                if (morph.Type == Morph.MorphType.MorphTypeVertex)
                {
                    vertexMorphs.Add(morph);
                }
            }

            Vector3[] modelVertexVectors = RawMMDModel.Vertices.Select(x => x.Coordinate).ToArray();
            Vector3[] modelVertexNormals = RawMMDModel.Vertices.Select(x => x.Normal).ToArray();
            List<(string name, Vector3[] offsets, Vector3[] normals)> blendShapes = new List<(string, Vector3[], Vector3[])>();
            await Task.Run(() =>
            {
                int vertexCount = RawMMDModel.Vertices.Length;
                int vertexMorphsCount = vertexMorphs.Count;
                for (int i = 0; i < vertexMorphsCount; i++)
                {
                    Vector3[] offsets = new Vector3[vertexCount];
                    for (int j = 0; j < vertexCount; j++)
                    {
                        offsets[j] = Vector3.zero;
                    }
                    Vector3[] normals = new Vector3[vertexCount];
                    for (int j = 0; j < vertexCount; j++)
                    {
                        normals[j] = Vector3.zero;
                    }
                    bool isEmpty = true;
                    int morphDataCount = vertexMorphs[i].MorphDatas.Length;
                    for (int j = 0; j < morphDataCount; j++)
                    {
                        Morph.VertexMorphData vertexMorphData = (vertexMorphs[i].MorphDatas[j] as Morph.VertexMorphData);
                        int vertexIndex = vertexMorphData.VertexIndex;
                        if (vertexIndex > vertexCount) { continue; }
                        offsets[vertexIndex] = vertexMorphData.Offset;
                        normals[vertexIndex] = RawMMDModel.Vertices[vertexIndex].Normal;
                        isEmpty = false;
                    }
                    if (!isEmpty)
                    {
                        blendShapes.Add((vertexMorphs[i].Name, offsets, normals));
                    }
                }
            });
            blendShapes.ForEach(x => mesh.AddBlendShapeFrame(x.name, 100f, x.offsets, x.normals, null));
        }

        private async Task SearchForMaterialMorph()
        {
            await Task.Run(() =>
            {
                foreach (Morph morph in RawMMDModel.Morphs)
                {
                    if (morph.Type == Morph.MorphType.MorphTypeMaterial)
                    {
                        MaterialMorphNames.Add(morph.Name);
                    }
                }
            });
        }

        private (GameObject rootObj, Transform[] bones) CreateBones(RawMMDModel model, GameObject parentOfAll)
        {
            if (model == null)
            {
                UnityEngine.Debug.Log("モデルがないため骨構造を作れません");
                return (null, null);
            }

            int boneCount = model.Bones.Length;
            Transform[] bones = new Transform[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                bones[i] = new GameObject(RawMMDModel.Bones[i].Name).transform;
                bones[i].transform.position = RawMMDModel.Bones[i].Position;
            }
            GameObject rootObj = BuildUpParentChildStructure(model, parentOfAll, bones);
            return (rootObj, bones);
        }

        GameObject BuildUpParentChildStructure(RawMMDModel model, GameObject parentOfAll, Transform[] bones)
        {
            GameObject rootObj = new GameObject("Root");
            ModelRootTransform = rootObj.transform;
            ModelRootTransform.parent = parentOfAll.transform;
            rootObj.transform.localPosition = Vector3.zero;
            rootObj.transform.localRotation = Quaternion.identity;
            rootObj.transform.localScale = Vector3.one;

            int boneCount = model.Bones.Length;
            for (int i = 0, iMax = boneCount; i < iMax; ++i)
            {
                var parentBoneIndex = model.Bones[i].ParentIndex;
                bones[i].parent = parentBoneIndex < bones.Length && parentBoneIndex >= 0
                    ? bones[parentBoneIndex].transform
                    : ModelRootTransform;
            }

            return rootObj;
        }

        private static BoneWeight ConvertBoneWeight(SkinningOperator op)
        {
            var ret = new BoneWeight();
            switch (op.Type)
            {
                case SkinningOperator.SkinningType.SkinningBdef1:
                    var bdef1 = (SkinningOperator.Bdef1)op.Param;
                    ret.boneIndex0 = bdef1.BoneId;
                    ret.weight0 = 1.0f;
                    break;
                case SkinningOperator.SkinningType.SkinningBdef2:
                    var bdef2 = (SkinningOperator.Bdef2)op.Param;
                    ret.boneIndex0 = bdef2.BoneId[0];
                    ret.boneIndex1 = bdef2.BoneId[1];
                    ret.weight0 = bdef2.BoneWeight;
                    ret.weight1 = 1 - bdef2.BoneWeight;
                    break;
                case SkinningOperator.SkinningType.SkinningBdef4:
                    var bdef4 = (SkinningOperator.Bdef4)op.Param;
                    ret.boneIndex0 = bdef4.BoneId[0];
                    ret.boneIndex1 = bdef4.BoneId[1];
                    ret.boneIndex2 = bdef4.BoneId[2];
                    ret.boneIndex3 = bdef4.BoneId[3];
                    ret.weight0 = bdef4.BoneWeight[0];
                    ret.weight1 = bdef4.BoneWeight[1];
                    ret.weight2 = bdef4.BoneWeight[2];
                    ret.weight3 = bdef4.BoneWeight[3];
                    break;
                case SkinningOperator.SkinningType.SkinningSdef:
                    var sdef = (SkinningOperator.Sdef)op.Param;
                    ret.boneIndex0 = sdef.BoneId[0];
                    ret.boneIndex1 = sdef.BoneId[1];
                    ret.weight0 = sdef.BoneWeight;
                    ret.weight1 = 1 - sdef.BoneWeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return ret;
        }

        private void BuildBindpose(Mesh mesh, Transform[] bones, SkinnedMeshRenderer meshRenderer)
        {
            Matrix4x4[] bindposes = bones.Select(x => x.worldToLocalMatrix).ToArray();
            mesh.bindposes = bindposes;
            meshRenderer.bones = bones;
        }

        private int GetRelBoneIndexFromNearbyRigidbody(int rigidbodyIndex)
        {
            var boneCount = RawMMDModel.Bones.Length;
            var result = RawMMDModel.Rigidbodies[rigidbodyIndex].AssociatedBoneIndex;
            if (result < boneCount)
            {
                return result;
            }
            var jointAList = RawMMDModel.Joints.Where(x => x.AssociatedRigidBodyIndex[1] == rigidbodyIndex)
                .Where(x => x.AssociatedRigidBodyIndex[0] < boneCount)
                .Select(x => x.AssociatedRigidBodyIndex[0]);
            foreach (var jointA in jointAList)
            {
                result = GetRelBoneIndexFromNearbyRigidbody(jointA);
                if (result < boneCount)
                {
                    return result;
                }
            }

            var jointBList = RawMMDModel.Joints.Where(x => x.AssociatedRigidBodyIndex[0] == rigidbodyIndex)
                .Where(x => x.AssociatedRigidBodyIndex[1] < boneCount)
                .Select(x => x.AssociatedRigidBodyIndex[1]);
            foreach (var jointB in jointBList)
            {
                result = GetRelBoneIndexFromNearbyRigidbody(jointB);
                if (result < boneCount)
                {
                    return result;
                }
            }

            result = int.MaxValue;
            return result;
        }

        private void ReorderRender(UnityEngine.Material[] materials)
        {
            if (materials.Length == 0)
            {
                return;
            }
            var order = new UnityEngine.Material[materials.Length];
            Array.Copy(materials, order, materials.Length);
            order.OrderBy(mat => mat.renderQueue);
            var lastQueue = int.MinValue;
            foreach (var mat in order)
            {
                if (lastQueue >= mat.renderQueue)
                {
                    mat.renderQueue = lastQueue++;
                }
                else
                {
                    lastQueue = mat.renderQueue;
                }
            }
        }

        public void ShowModel()
        {
            if (SkinnedMeshRenderer == null) { return; }

            SkinnedMeshRenderer.enabled = true;
        }

        public void HideModel()
        {
            if (SkinnedMeshRenderer == null) { return; }

            SkinnedMeshRenderer.enabled = false;
        }
    }
}