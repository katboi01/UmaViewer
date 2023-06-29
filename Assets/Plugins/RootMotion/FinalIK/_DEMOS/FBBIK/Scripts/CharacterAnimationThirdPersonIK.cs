using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {
	
	/// <summary>
	/// Contols animation for a third person person controller.
	/// </summary>
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class CharacterAnimationThirdPersonIK: CharacterAnimationThirdPerson {
		
		private FullBodyBipedIK ik;

		protected override void Start() {
			base.Start();
			
			ik = GetComponent<FullBodyBipedIK>();
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			// Rotate the upper body a little bit to world up vector if the character is rotated (for wall-running)
			if (Vector3.Angle(transform.up, Vector3.up) <= 0.01f) return;
			
			Quaternion r = Quaternion.FromToRotation(transform.up, Vector3.up);
			
			RotateEffector(ik.solver.bodyEffector, r, 0.1f);
			RotateEffector(ik.solver.leftShoulderEffector, r, 0.2f);
			RotateEffector(ik.solver.rightShoulderEffector, r, 0.2f);
			RotateEffector(ik.solver.leftHandEffector, r, 0.1f);
			RotateEffector(ik.solver.rightHandEffector, r, 0.1f);
		}
		
		// Rotate an effector from the root of the character
		private void RotateEffector(IKEffector effector, Quaternion rotation, float mlp) {
			Vector3 d1 = effector.bone.position - transform.position;
			Vector3 d2 = rotation * d1;
			Vector3 offset = d2 - d1;
			effector.positionOffset += offset * mlp;
		}
	}
}
