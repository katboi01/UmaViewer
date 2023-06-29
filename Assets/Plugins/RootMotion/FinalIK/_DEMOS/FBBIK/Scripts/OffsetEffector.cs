using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Custom positionOffset effector for FBBIK, could be used for example to make a spine or pelvis effector.
	/// </summary>
	public class OffsetEffector : OffsetModifier {

		[System.Serializable]
		public class EffectorLink {
			public FullBodyBipedEffector effectorType;
			public float weightMultiplier = 1f;

			[HideInInspector] public Vector3 localPosition;
		}

		public EffectorLink[] effectorLinks;

		protected override void Start() {
			base.Start();

			// Store the default positions of the effectors relative to this GameObject's position
			foreach (EffectorLink e in effectorLinks) {
				e.localPosition = transform.InverseTransformPoint(ik.solver.GetEffector(e.effectorType).bone.position);

				// If we are using the body effector, make sure it does not change the thigh effectors
				if (e.effectorType == FullBodyBipedEffector.Body) ik.solver.bodyEffector.effectChildNodes = false;
			}
		}

		protected override void OnModifyOffset() {
			// Update the effectors
			foreach (EffectorLink e in effectorLinks) {
				// Using effector positionOffset
				Vector3 positionTarget = transform.TransformPoint(e.localPosition);

				ik.solver.GetEffector(e.effectorType).positionOffset += (positionTarget - (ik.solver.GetEffector(e.effectorType).bone.position + ik.solver.GetEffector(e.effectorType).positionOffset)) * weight * e.weightMultiplier;
			}
		}
	}
}
