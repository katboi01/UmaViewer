using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LibMMD.Unity3D;
using System.Diagnostics;
using System.Threading.Tasks;
namespace LibMMD.Unity3D
{
    public class AvatarMaker : MonoBehaviour
    {
        public RuntimeAnimatorController RuntimeAnimatorController;

        const string AvatarNameTail = "Avatar";

        private MMDModel model;
        private SkinnedMeshRenderer skinnedMeshRenderer;
        //BuildAvatarするまでnull
        private Animator animator;

        Dictionary<string, Transform> humanBoneDictionary = new Dictionary<string, Transform>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Prepare(MMDModel mmdModel, RuntimeAnimatorController runtimeAnimatorController)
        {
            model = mmdModel;
            RuntimeAnimatorController = runtimeAnimatorController;
            humanBoneDictionary = MakeHumanBoneDictionary(transform);
            skinnedMeshRenderer = mmdModel.SkinnedMeshRenderer;
        }

        public void Prepare(MMDModel mmdModel)
        {
            Prepare(mmdModel, null);
        }

        public async Task<bool> MakeAvatar()
        {
            bool fixedBone = await FixBoneWeight();
            if (!fixedBone)
            {
                UnityEngine.Debug.Log("Failed to fix bone.");
                return false;
            }
            EnforceTPose();
            return await BuildAvatar();
        }

        private void EnforceTPose()
        {
            Vector3 right = transform.right;
            Vector3 left = -right;

            Vector3 rightUpperArmToLowerArm = humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightLowerArm]].position - humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightUpperArm]].position;
            Vector3 leftUpperArmToLowerArm = humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftLowerArm]].position - humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftUpperArm]].position;
            humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightUpperArm]].Rotate(Vector3.Cross(rightUpperArmToLowerArm, right), Vector3.Angle(rightUpperArmToLowerArm, right));
            humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftUpperArm]].Rotate(Vector3.Cross(leftUpperArmToLowerArm, left), Vector3.Angle(leftUpperArmToLowerArm, left));

            Vector3 leftHandToMiddleFinger = humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftMiddleProximal]].position - humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftHand]].position;
            Vector3 rightHandToMiddleFinger = humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightMiddleProximal]].position - humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightHand]].position;
            Vector3 projectedLeftHandToMiddleFinger = Vector3.ProjectOnPlane(leftHandToMiddleFinger, transform.up);
            Vector3 projectedRightHandToMiddleFinger = Vector3.ProjectOnPlane(rightHandToMiddleFinger, transform.up);
            humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftHand]].Rotate(Vector3.Cross(leftHandToMiddleFinger, projectedLeftHandToMiddleFinger), Vector3.Angle(leftHandToMiddleFinger, projectedLeftHandToMiddleFinger));
            humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightHand]].Rotate(Vector3.Cross(rightHandToMiddleFinger, projectedRightHandToMiddleFinger), Vector3.Angle(rightHandToMiddleFinger, projectedRightHandToMiddleFinger));

            rightHandToMiddleFinger = humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightMiddleProximal]].position - humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightHand]].position;
            leftHandToMiddleFinger = humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftMiddleProximal]].position - humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftHand]].position;
            humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.RightHand]].Rotate(-Vector3.Cross(rightHandToMiddleFinger, right), Vector3.Angle(rightHandToMiddleFinger, right));
            humanBoneDictionary[MMDModel.Unity_MMDBoneNameDictionary[MMDModel.LeftHand]].Rotate(-Vector3.Cross(leftHandToMiddleFinger, left), Vector3.Angle(leftHandToMiddleFinger, left));
        }

        private async Task<bool> BuildAvatar()
        {
            // HumanBoneのためのリストを取得する
            List<HumanBone> humanBones = new List<HumanBone>();
            string[] boneNames = HumanTrait.BoneName;
            await Task.Run(() =>
            {
                foreach (string humanBoneName in boneNames)
                {
                    if (!MMDModel.Unity_MMDBoneNameDictionary.ContainsKey(humanBoneName)) { continue; }
                    if (MMDModel.Unity_MMDBoneNameDictionary[humanBoneName] == null) { continue; }
                    HumanBone humanBone = new HumanBone();
                    humanBone.humanName = humanBoneName;
                    humanBone.boneName = MMDModel.Unity_MMDBoneNameDictionary[humanBoneName];
                    humanBone.limit.useDefaultValues = true;
                    humanBones.Add(humanBone);
                }
            });
            // HumanDescription（関節の曲がり方などを定義した構造体）
            HumanDescription humanDesc = new HumanDescription();
            humanDesc.human = humanBones.ToArray();
            humanDesc.skeleton = GetTransforms(transform).ToArray();

            humanDesc.upperArmTwist = 0.5f;
            humanDesc.lowerArmTwist = 0.5f;
            humanDesc.upperLegTwist = 0.5f;
            humanDesc.lowerLegTwist = 0.5f;
            humanDesc.armStretch = 0.05f;
            humanDesc.legStretch = 0.05f;
            humanDesc.feetSpacing = 0.05f;

            // アバターオブジェクトをビルド
            Avatar avatar = AvatarBuilder.BuildHumanAvatar(gameObject, humanDesc);
            avatar.name = gameObject.name + AvatarNameTail;
            if (!avatar.isValid || !avatar.isHuman)
            {
                UnityEngine.Debug.Log("Error when building avatar");
                return false;
            }
            animator = gameObject.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.applyRootMotion = true;
            animator.runtimeAnimatorController = RuntimeAnimatorController;
            return true;
        }

        private List<SkeletonBone> GetTransforms(Transform t)
        {
            SkeletonBone skeletonBone = new SkeletonBone()
            {
                name = t.name,
                position = t.localPosition,
                rotation = t.localRotation,
                scale = t.localScale
            };

            List<SkeletonBone> singleParentFamily = new List<SkeletonBone>() { skeletonBone };

            foreach (Transform childT in t)
            {
                singleParentFamily.AddRange(GetTransforms(childT));
            }

            return singleParentFamily;
        }

        private async Task<bool> FixBoneWeight()
        {
            return await FixBoneWeight(skinnedMeshRenderer);
        }

        private async Task<bool> FixBoneWeight(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (skinnedMeshRenderer == null)
            {
                UnityEngine.Debug.Log("No SkinnedMeshRenderer.");
                return false;
            }
            if (skinnedMeshRenderer.sharedMesh == null)
            {
                UnityEngine.Debug.Log("No SharedMesh.");
                return false;
            }

            Transform[] bones = skinnedMeshRenderer.bones;

            string[] boneNames = (from t in bones select t.name).ToArray();

            BoneWeight[] fixedBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
            int vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;

            await Task.Run(() =>
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    if (model.Alt_OriginalBoneDictionary.ContainsKey(bones[fixedBoneWeights[i].boneIndex0]))
                    {
                        fixedBoneWeights[i].boneIndex0
                            = model.OriginalBone_IndexDictionary[model.Alt_OriginalBoneDictionary[bones[fixedBoneWeights[i].boneIndex0]]];
                    }

                    if (model.Alt_OriginalBoneDictionary.ContainsKey(bones[fixedBoneWeights[i].boneIndex1]))
                    {
                        fixedBoneWeights[i].boneIndex1
                            = model.OriginalBone_IndexDictionary[model.Alt_OriginalBoneDictionary[bones[fixedBoneWeights[i].boneIndex1]]];
                    }

                    if (model.Alt_OriginalBoneDictionary.ContainsKey(bones[fixedBoneWeights[i].boneIndex2]))
                    {
                        fixedBoneWeights[i].boneIndex2
                            = model.OriginalBone_IndexDictionary[model.Alt_OriginalBoneDictionary[bones[fixedBoneWeights[i].boneIndex2]]];
                    }

                    if (model.Alt_OriginalBoneDictionary.ContainsKey(bones[fixedBoneWeights[i].boneIndex3]))
                    {
                        fixedBoneWeights[i].boneIndex3
                            = model.OriginalBone_IndexDictionary[model.Alt_OriginalBoneDictionary[bones[fixedBoneWeights[i].boneIndex3]]];
                    }

                    if (MMDModel.Alt_OriginalNameDictionary.ContainsKey(boneNames[fixedBoneWeights[i].boneIndex0]))
                    {
                        string originalBoneName = MMDModel.Alt_OriginalNameDictionary[boneNames[fixedBoneWeights[i].boneIndex0]];
                        if (model.OriginalBone_IndexDictionary.ContainsKey(humanBoneDictionary[originalBoneName]))
                        {
                            fixedBoneWeights[i].boneIndex0 = model.OriginalBone_IndexDictionary[humanBoneDictionary[originalBoneName]];
                        }
                    }

                    if (MMDModel.Alt_OriginalNameDictionary.ContainsKey(boneNames[fixedBoneWeights[i].boneIndex1]))
                    {
                        string originalBoneName = MMDModel.Alt_OriginalNameDictionary[boneNames[fixedBoneWeights[i].boneIndex1]];
                        if (model.OriginalBone_IndexDictionary.ContainsKey(humanBoneDictionary[originalBoneName]))
                        {
                            fixedBoneWeights[i].boneIndex1 = model.OriginalBone_IndexDictionary[humanBoneDictionary[originalBoneName]];
                        }
                    }

                    if (MMDModel.Alt_OriginalNameDictionary.ContainsKey(boneNames[fixedBoneWeights[i].boneIndex2]))
                    {
                        string originalBoneName = MMDModel.Alt_OriginalNameDictionary[boneNames[fixedBoneWeights[i].boneIndex2]];
                        if (model.OriginalBone_IndexDictionary.ContainsKey(humanBoneDictionary[originalBoneName]))
                        {
                            fixedBoneWeights[i].boneIndex2 = model.OriginalBone_IndexDictionary[humanBoneDictionary[originalBoneName]];
                        }
                    }

                    if (MMDModel.Alt_OriginalNameDictionary.ContainsKey(boneNames[fixedBoneWeights[i].boneIndex3]))
                    {
                        string originalBoneName = MMDModel.Alt_OriginalNameDictionary[boneNames[fixedBoneWeights[i].boneIndex3]];
                        if (model.OriginalBone_IndexDictionary.ContainsKey(humanBoneDictionary[originalBoneName]))
                        {
                            fixedBoneWeights[i].boneIndex3 = model.OriginalBone_IndexDictionary[humanBoneDictionary[originalBoneName]];
                        }
                    }
                }
            });

            skinnedMeshRenderer.sharedMesh.boneWeights = fixedBoneWeights;

            return true;
        }

        private Dictionary<string, Transform> MakeHumanBoneDictionary(Transform rootTransform, string[] humanBonesContained)
        {
            Dictionary<string, Transform> humanBoneDictionary = new Dictionary<string, Transform>();
            Queue queue = new Queue();
            queue.Enqueue(rootTransform);
            while (queue.Count != 0)
            {
                Transform peek = queue.Peek() as Transform;
                if (MMDModel.Unity_MMDBoneNameDictionary.ContainsValue(peek.name) && !humanBoneDictionary.ContainsKey(peek.name))
                {
                    humanBoneDictionary.Add(peek.name, peek);
                }

                if (humanBonesContained.All(x => x != null && humanBoneDictionary.ContainsKey(x)))
                {
                    break;
                }

                foreach (Transform childT in (queue.Dequeue() as Transform))
                {
                    queue.Enqueue(childT);
                }
            }

            return humanBoneDictionary;
        }
        private Dictionary<string, Transform> MakeHumanBoneDictionary(Transform rootTransform)
        {
            string[] allBones = (from boneName in HumanTrait.BoneName select MMDModel.Unity_MMDBoneNameDictionary[boneName]).ToArray();
            return MakeHumanBoneDictionary(rootTransform, allBones);
        }
    }
}