using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Demo script that amplifies the motion of a body part relative to the root of the character or another body part.
	/// </summary>
	public class Amplifier: OffsetModifier {

		/// <summary>
		/// Body is amplifying the motion of "transform" relative to the "relativeTo".
		/// </summary>
		[System.Serializable]
		public class Body {

			/// <summary>
			/// Linking this to an effector
			/// </summary>
			[System.Serializable]
			public class EffectorLink {
				[Tooltip("Type of the FBBIK effector to use")]
				public FullBodyBipedEffector effector;
				[Tooltip("Weight of using this effector")]
				public float weight;
			}

			[Tooltip("The Transform that's motion we are reading.")]
			public Transform transform;
			[Tooltip("Amplify the 'transform's' position relative to this Transform.")]
			public Transform relativeTo;
			[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector.")]
			public EffectorLink[] effectorLinks;
			[Tooltip("Amplification magnitude along the up axis of the character.")]
			public float verticalWeight = 1f;
			[Tooltip("Amplification magnitude along the horizontal axes of the character.")]
			public float horizontalWeight = 1f;
			[Tooltip("Speed of the amplifier. 0 means instant.")]
			public float speed = 3f;

			private Vector3 lastRelativePos;
			private Vector3 smoothDelta;
			private bool firstUpdate;

			// Update the Body
			public void Update(IKSolverFullBodyBiped solver, float w, float deltaTime) {
				if (transform == null || relativeTo == null) return;

				// Find the relative position of the transform
				Vector3 relativePos = relativeTo.InverseTransformDirection(transform.position - relativeTo.position);

				// Initiating
				if (firstUpdate) {
					lastRelativePos = relativePos;
					firstUpdate = false;
				}

				// Find how much the relative position has changed
				Vector3 delta = (relativePos - lastRelativePos) / deltaTime;

				// Smooth the change
				smoothDelta = speed <= 0f? delta: Vector3.Lerp(smoothDelta, delta, deltaTime * speed);

				// Convert to world space
				Vector3 worldDelta = relativeTo.TransformDirection(smoothDelta);

				// Extract horizontal and vertical offset
				Vector3 offset = V3Tools.ExtractVertical(worldDelta, solver.GetRoot().up, verticalWeight) + V3Tools.ExtractHorizontal(worldDelta, solver.GetRoot().up, horizontalWeight);

				// Apply the amplitude to the effector links
				for (int i = 0; i < effectorLinks.Length; i++) {
					solver.GetEffector(effectorLinks[i].effector).positionOffset += offset * w * effectorLinks[i].weight;
				}

				lastRelativePos = relativePos;
			}

			// Multiply 2 vectors
			private static Vector3 Multiply(Vector3 v1, Vector3 v2) {
				v1.x *= v2.x;
				v1.y *= v2.y;
				v1.z *= v2.z;
				return v1;
			}
		}

		[Tooltip("The amplified bodies.")]
		public Body[] bodies;

		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			if (!ik.fixTransforms) {
				if (!Warning.logged) Warning.Log("Amplifier needs the Fix Transforms option of the FBBIK to be set to true. Otherwise it might amplify to infinity, should the animator of the character stop because of culling.", transform);
				return;
			}

			// Update the Bodies
			foreach (Body body in bodies) body.Update(ik.solver, weight, deltaTime);
		}
	}
}
