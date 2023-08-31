using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Grounding for BipedIK characters.
	/// </summary>
	[HelpURL("http://www.root-motion.com/finalikdox/html/page9.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Biped")]
	public class GrounderBipedIK: Grounder {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_biped_i_k.html");
		}

		#region Main Interface
		
		/// <summary>
		/// The BipedIK componet.
		/// </summary>
		[Tooltip("The BipedIK componet.")]
		public BipedIK ik;
		/// <summary>
		/// The amount of spine bending towards upward slopes.
		/// </summary>
		[Tooltip("The amount of spine bending towards upward slopes.")]
		public float spineBend = 7f;
		/// <summary>
		/// The interpolation speed of spine bending.
		/// </summary>
		[Tooltip("The interpolation speed of spine bending.")]
		public float spineSpeed = 3f;

		#endregion Main Interface

		public override void ResetPosition() {
			solver.Reset();
			spineOffset = Vector3.zero;
		}
		
		private Transform[] feet = new Transform[2];
		private Quaternion[] footRotations = new Quaternion[2];
		private Vector3 animatedPelvisLocalPosition, solvedPelvisLocalPosition;
		private Vector3 spineOffset;
		private float lastWeight;

		// Can we initiate the Grounding?
		private bool IsReadyToInitiate() {
			if (ik == null) return false;
			if (!ik.solvers.leftFoot.initiated) return false;
			if (!ik.solvers.rightFoot.initiated) return false;
			return true;
		}

		// Initiate once we have a BipedIK component
		void Update() {
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (weight <= 0f) return;

			if (initiated) return;
			if (!IsReadyToInitiate()) return;
			
			Initiate();
		}
		
		private void Initiate() {
			// Gathering both foot bones from the BipedIK
			feet = new Transform[2];
			footRotations = new Quaternion[2];

			feet[0] = ik.references.leftFoot;
			feet[1] = ik.references.rightFoot;

			footRotations[0] = Quaternion.identity;
			footRotations[1] = Quaternion.identity;

			// Adding to the delegates to get call at certain points in the solving process
			ik.solvers.spine.OnPreUpdate += OnSolverUpdate;
			ik.solvers.rightFoot.OnPostUpdate += OnPostSolverUpdate;

			// Store the default localPosition of the pelvis
			animatedPelvisLocalPosition = ik.references.pelvis.localPosition;

			// Initiate the Grounding
			solver.Initiate(ik.references.root, feet);
			
			initiated = true;
		}

		// Weigh out the limb solvers properly when the component is disabled
		void OnDisable() {
			if (!initiated) return;

			ik.solvers.leftFoot.IKPositionWeight = 0f;
			ik.solvers.rightFoot.IKPositionWeight = 0f;
		}

		// Called before updating the spine IK solver
		private void OnSolverUpdate() {
			if (!enabled) return;

			if (weight <= 0f) {
				if (lastWeight <= 0f) return;

				// Weigh out the limb solvers properly
				OnDisable();
			}

			lastWeight = weight;

			if (OnPreGrounder != null) OnPreGrounder();

			// If the pelvis local position has not changed since last solved state, consider it unanimated
			if (ik.references.pelvis.localPosition != solvedPelvisLocalPosition) animatedPelvisLocalPosition = ik.references.pelvis.localPosition;
			else ik.references.pelvis.localPosition = animatedPelvisLocalPosition;

			// Update the Grounding
			solver.Update();

			// Move the pelvis
			ik.references.pelvis.position += solver.pelvis.IKOffset * weight;

			// Update IKPositions and IKPositionWeights of the feet
			SetLegIK(ik.solvers.leftFoot, 0);
			SetLegIK(ik.solvers.rightFoot, 1);

			// Bending the spine
			if (spineBend != 0f && ik.references.spine.Length > 0) {
				spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);

				Vector3 spineOffseTarget = GetSpineOffsetTarget() * weight;
				spineOffset = Vector3.Lerp(spineOffset, spineOffseTarget * spineBend, Time.deltaTime * spineSpeed);

				// Store upper arm rotations to revert them after we rotate the spine
				Quaternion leftArmRotation = ik.references.leftUpperArm.rotation;
				Quaternion rightArmRotation = ik.references.rightUpperArm.rotation;

				// Get the offset rotation for the spine
				Vector3 up = solver.up;
				Quaternion f = Quaternion.FromToRotation(up, up + spineOffset);

				// Rotate the spine
				ik.references.spine[0].rotation = f * ik.references.spine[0].rotation;

				// Revert the upper arms
				ik.references.leftUpperArm.rotation = leftArmRotation;
				ik.references.rightUpperArm.rotation = rightArmRotation;
			}

			if (OnPostGrounder != null) OnPostGrounder();
		}

		// Set the IK position and weight for a limb
		private void SetLegIK(IKSolverLimb limb, int index) {
			footRotations[index] = feet[index].rotation;

			limb.IKPosition = solver.legs[index].IKPosition;
			limb.IKPositionWeight = weight;
		}

		// Rotating the feet after IK has finished
		private void OnPostSolverUpdate() {
			if (weight <= 0f) return;
			if (!enabled) return;

			for (int i = 0; i < feet.Length; i++) {
				feet[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) * footRotations[i];
			}

			// Store the local position of the pelvis so we know it it changes
			solvedPelvisLocalPosition = ik.references.pelvis.localPosition;
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (initiated && ik != null) {
				ik.solvers.spine.OnPreUpdate -= OnSolverUpdate;
				ik.solvers.rightFoot.OnPostUpdate -= OnPostSolverUpdate;
			}
		}
	}
}

