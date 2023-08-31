using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Relaxes the twist rotation of this Transform relative to a parent and a child Transform, using their initial rotations as the most relaxed pose.
	/// </summary>
	public class TwistRelaxer : MonoBehaviour {

		public IK ik;

        [Tooltip("If this is the forearm roll bone, the parent should be the forearm bone. If null, will be found automatically.")]
        public Transform parent;

        [Tooltip("If this is the forearm roll bone, the child should be the hand bone. If null, will attempt to find automatically. Assign the hand manually if the hand bone is not a child of the roll bone.")]
        public Transform child;

        [Tooltip("The weight of relaxing the twist of this Transform")] 
		[Range(0f, 1f)] public float weight = 1f;

		[Tooltip("If 0.5, this Transform will be twisted half way from parent to child. If 1, the twist angle will be locked to the child and will rotate with along with it.")]
		[Range(0f, 1f)] public float parentChildCrossfade = 0.5f;

        [Tooltip("Rotation offset around the twist axis.")]
        [Range(-180f, 180f)] public float twistAngleOffset;

		/// <summary>
		/// Rotate this Transform to relax it's twist angle relative to the "parent" and "child" Transforms.
		/// </summary>
		public void Relax() {
			if (weight <= 0f) return; // Nothing to do here

            Quaternion rotation = transform.rotation;
            Quaternion twistOffset = Quaternion.AngleAxis(twistAngleOffset, rotation * twistAxis);
            rotation = twistOffset * rotation;
			
			// Find the world space relaxed axes of the parent and child
			Vector3 relaxedAxisParent = twistOffset * parent.rotation * axisRelativeToParentDefault;
			Vector3 relaxedAxisChild = twistOffset * child.rotation * axisRelativeToChildDefault;
			
			// Cross-fade between the parent and child
			Vector3 relaxedAxis = Vector3.Slerp(relaxedAxisParent, relaxedAxisChild, parentChildCrossfade);
			
			// Convert relaxedAxis to (axis, twistAxis) space so we could calculate the twist angle
			Quaternion r = Quaternion.LookRotation(rotation * axis, rotation * twistAxis);
			relaxedAxis = Quaternion.Inverse(r) * relaxedAxis;
			
			// Calculate the angle by which we need to rotate this Transform around the twist axis.
			float angle = Mathf.Atan2(relaxedAxis.x, relaxedAxis.z) * Mathf.Rad2Deg;
			
			// Store the rotation of the child so it would not change with twisting this Transform
			Quaternion childRotation = child.rotation;
			
			// Twist the bone
			transform.rotation = Quaternion.AngleAxis(angle * weight, rotation * twistAxis) * rotation;
			
			// Revert the rotation of the child
			child.rotation = childRotation;
		}

        private Vector3 twistAxis = Vector3.right;
		private Vector3 axis = Vector3.forward;
		private Vector3 axisRelativeToParentDefault, axisRelativeToChildDefault;
		
		void Start() {
			if (parent == null) parent = transform.parent;

            if (child == null)
            {
                if (transform.childCount == 0)
                {
                    var children = parent.GetComponentsInChildren<Transform>();
                    for (int i = 1; i < children.Length; i++)
                    {
                        if (children[i] != transform)
                        {
                            child = children[i];
                            break;
                        }
                    }
                }
                else
                {
                    child = transform.GetChild(0);
                }
            }

			twistAxis = transform.InverseTransformDirection(child.position - transform.position);
			axis = new Vector3(twistAxis.y, twistAxis.z, twistAxis.x);

			// Axis in world space
			Vector3 axisWorld = transform.rotation * axis;

			// Store the axis in worldspace relative to the rotations of the parent and child
			axisRelativeToParentDefault = Quaternion.Inverse(parent.rotation) * axisWorld;
			axisRelativeToChildDefault = Quaternion.Inverse(child.rotation) * axisWorld;

			if (ik != null) ik.GetIKSolver().OnPostUpdate += OnPostUpdate;
		}

		void OnPostUpdate() {
			if (ik != null) Relax();
		}

		void LateUpdate() {
			if (ik == null) Relax();
		}

		void OnDestroy() {
			if (ik != null) ik.GetIKSolver().OnPostUpdate -= OnPostUpdate;
		}
	}
}
