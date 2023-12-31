using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Calculates bending direction and hand rotations for a FBBIK rig for VR hand controllers. 
	/// </summary>
	public class FBBIKArmBending : MonoBehaviour {

		public FullBodyBipedIK ik;

		// Bend direction offset for the arms
		public Vector3 bendDirectionOffsetLeft;
		public Vector3 bendDirectionOffsetRight;

		// Add some bend direction offset in character space
		public Vector3 characterSpaceBendOffsetLeft;
		public Vector3 characterSpaceBendOffsetRight;

		private Quaternion leftHandTargetRotation;
		private Quaternion rightHandTargetRotation;
		private bool initiated;

		void LateUpdate() {
			if (ik == null) return;

			if (!initiated) {
				ik.solver.OnPostUpdate += OnPostFBBIK;
				initiated = true;
			}
			
			// Left arm bend direction
			if (ik.solver.leftHandEffector.target != null) {
				Vector3 armAxisLeft = Vector3.left;
				ik.solver.leftArmChain.bendConstraint.direction = ik.solver.leftHandEffector.target.rotation * armAxisLeft +  ik.solver.leftHandEffector.target.rotation * bendDirectionOffsetLeft + ik.transform.rotation * characterSpaceBendOffsetLeft;
				ik.solver.leftArmChain.bendConstraint.weight = 1f;
			}

			// Right arm bend direction
			if (ik.solver.rightHandEffector.target != null) {
				Vector3 armAxisRight = Vector3.right;
				ik.solver.rightArmChain.bendConstraint.direction = ik.solver.rightHandEffector.target.rotation * armAxisRight +  ik.solver.rightHandEffector.target.rotation * bendDirectionOffsetRight + ik.transform.rotation * characterSpaceBendOffsetRight;
				ik.solver.rightArmChain.bendConstraint.weight = 1f;
			}
		}

		void OnPostFBBIK() {
			if (ik == null) return;

			// Rotate hand bones
			if (ik.solver.leftHandEffector.target != null) {
				ik.references.leftHand.rotation = ik.solver.leftHandEffector.target.rotation;
			}

			if (ik.solver.rightHandEffector.target != null) {
				ik.references.rightHand.rotation = ik.solver.rightHandEffector.target.rotation;
			}
		}

		void OnDestroy() {
			if (ik != null) ik.solver.OnPostUpdate -= OnPostFBBIK;
		}
	}
}
