using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.Demos
{
    // Adds simple FK rotation offset to bones.
    public class FKOffset : MonoBehaviour
    {
        [System.Serializable]
        public class Offset
        {
            [HideInInspector][SerializeField] public string name;
            public HumanBodyBones bone;
            public Vector3 rotationOffset;

            private Transform t;

            public void Apply(Animator animator)
            {
                if (t == null) t = animator.GetBoneTransform(bone);
                if (t == null) return;

                t.localRotation *= Quaternion.Euler(rotationOffset);
            }
        }

        public Offset[] offsets;

        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            foreach (Offset offset in offsets)
            {
                offset.Apply(animator);
            }
        }

        private void OnDrawGizmosSelected()
        {
            foreach (Offset offset in offsets)
            {
                offset.name = offset.bone.ToString();
            }
        }
    }
}
