using UnityEngine;
using System.Collections;
using System;

namespace RootMotion
{
    /// <summary>
    /// Baker for Generic/Legacy animation.
    /// </summary>
    public class GenericBaker : Baker
    {
        /// <summary>
        /// If true, produced AnimationClips will be marked as Legacy and usable with the Legacy animation system.
        /// </summary>
        [Tooltip("If true, produced AnimationClips will be marked as Legacy and usable with the Legacy animation system.")]
        public bool markAsLegacy;

        /// <summary>
        /// Root Transform of the hierarchy to bake.
        /// </summary>
        [Tooltip("Root Transform of the hierarchy to bake.")]
        public Transform root;

        /// <summary>
        /// Root Node used for root motion.
        /// </summary>
        [Tooltip("Root Node used for root motion.")]
        public Transform rootNode;

        /// <summary>
        /// List of Transforms to ignore, rotation curves will not be baked for these Transforms.
        /// </summary>
        [Tooltip("List of Transforms to ignore, rotation curves will not be baked for these Transforms.")]
        public Transform[] ignoreList;

        /// <summary>
        /// LocalPosition curves will be baked for these Transforms only. If you are baking a character, the pelvis bone should be added to this array.
        /// </summary>
        [Tooltip("LocalPosition curves will be baked for these Transforms only. If you are baking a character, the pelvis bone should be added to this array.")]
        public Transform[] bakePositionList;

        private BakerTransform[] children = new BakerTransform[0];
        private BakerTransform rootChild;
        private int rootChildIndex = -1;

        void Awake()
        {
            // Find all the child Transforms of the Animator
            Transform[] childrenAndRoot = (Transform[])root.GetComponentsInChildren<Transform>();

            children = new BakerTransform[0];

            // Exlude the ignore list, construct the children array
            for (int i = 0; i < childrenAndRoot.Length; i++)
            {
                if (!IsIgnored(childrenAndRoot[i]))
                {

                    Array.Resize(ref children, children.Length + 1);

                    bool isRootNode = childrenAndRoot[i] == rootNode;
                    if (isRootNode) rootChildIndex = children.Length - 1;

                    children[children.Length - 1] = new BakerTransform(childrenAndRoot[i], root, BakePosition(childrenAndRoot[i]), isRootNode);
                }
            }
        }

        protected override Transform GetCharacterRoot()
        {
            return root;
        }

        protected override void OnStartBaking()
        {
            for (int i = 0; i < children.Length; i++)
            {
                children[i].Reset();

                if (i == rootChildIndex) children[i].SetRelativeSpace(root.position, root.rotation);
            }
        }

        protected override void OnSetLoopFrame(float time)
        {
            // TODO Change to SetLoopFrame like in HumanoidBaker
            for (int i = 0; i < children.Length; i++) children[i].AddLoopFrame(time);
        }

        protected override void OnSetCurves(ref AnimationClip clip)
        {
            // TODO Length Multiplier

            for (int i = 0; i < children.Length; i++) children[i].SetCurves(ref clip);
        }

        protected override void OnSetKeyframes(float time, bool lastFrame)
        {
            for (int i = 0; i < children.Length; i++) children[i].SetKeyframes(time);
        }

        // Is the specified Transform in the ignore list?
        private bool IsIgnored(Transform t)
        {
            for (int i = 0; i < ignoreList.Length; i++)
            {
                if (t == ignoreList[i]) return true;
            }
            return false;
        }

        // Should we record the localPosition channels of the Transform? 
        private bool BakePosition(Transform t)
        {
            for (int i = 0; i < bakePositionList.Length; i++)
            {
                if (t == bakePositionList[i]) return true;
            }
            return false;
        }

#if UNITY_EDITOR
        protected override void SetClipSettings(AnimationClip clip, UnityEditor.AnimationClipSettings settings)
        {
            clip.legacy = markAsLegacy;

            if (mode != Baker.Mode.AnimationClips)
            {
                clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Default;
            }
        }
#endif
    }
}
