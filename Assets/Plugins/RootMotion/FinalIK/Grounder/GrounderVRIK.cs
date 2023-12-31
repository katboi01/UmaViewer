using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Grounding for FBBIK characters.
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder VRIK")]
	public class GrounderVRIK: Grounder {

		// Open a video tutorial video
		[ContextMenu("TUTORIAL VIDEO")]
		void OpenTutorial() {
			Application.OpenURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_v_r_i_k.html");
		}

		#region Main Interface

		/// <summary>
		/// Reference to the VRIK componet.
		/// </summary>
		[Tooltip("Reference to the VRIK componet.")]
		public VRIK ik;

		#endregion Main Interface

		public override void ResetPosition() {
			solver.Reset();
		}

		private Transform[] feet = new Transform[2];

		// Can we initiate the Grounding?
		private bool IsReadyToInitiate() {
			if (ik == null) return false;
			if (!ik.solver.initiated) return false;
			return true;
		}

		// Initiate once we have a FBBIK component
		void Update() {
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (weight <= 0f) return;

			if (initiated) return;
			if (!IsReadyToInitiate()) return;
			
			Initiate();
		}

		private void Initiate () {
			// Gathering both foot bones from the FBBIK
			feet = new Transform[2];
			feet[0] = ik.references.leftFoot;
			feet[1] = ik.references.rightFoot;

			// Add to the FBBIK OnPreUpdate delegate to know when it solves
			ik.solver.OnPreUpdate += OnSolverUpdate;
			ik.solver.OnPostUpdate += OnPostSolverUpdate;

			// Initiate Grounding
			solver.Initiate(ik.references.root, feet);

			initiated = true;
		}

		// Called before updating the main IK solver
		private void OnSolverUpdate() {
			if (!enabled) return;
			if (weight <= 0f) return;

			if (OnPreGrounder != null) OnPreGrounder();

			/*
			Vector3 leftFootPos = ik.references.leftFoot.position;
			Vector3 rightFootPos = ik.references.rightFoot.position;

			ik.references.leftFoot.position = ik.solver.locomotion.leftFootstepPosition;
			ik.references.rightFoot.position = ik.solver.locomotion.rightFootstepPosition;
			*/
			solver.Update();

			//ik.references.leftFoot.position = leftFootPos;
			//ik.references.rightFoot.position = rightFootPos;

			// Move the pelvis
			ik.references.pelvis.position += solver.pelvis.IKOffset * weight;

			// Set effector positionOffsets for the feet
			ik.solver.AddPositionOffset (IKSolverVR.PositionOffset.LeftFoot, (solver.legs[0].IKPosition - ik.references.leftFoot.position) * weight);
			ik.solver.AddPositionOffset (IKSolverVR.PositionOffset.RightFoot, (solver.legs[1].IKPosition - ik.references.rightFoot.position) * weight);

			if (OnPostGrounder != null) OnPostGrounder();
		}
		
		// Set the effector positionOffset for the foot
		private void SetLegIK(IKSolverVR.PositionOffset positionOffset, Transform bone, Grounding.Leg leg) {
			ik.solver.AddPositionOffset (positionOffset, (leg.IKPosition - bone.position) * weight);
		}

		void OnPostSolverUpdate() {
			ik.references.leftFoot.rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[0].rotationOffset, weight) * ik.references.leftFoot.rotation;
			ik.references.rightFoot.rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[1].rotationOffset, weight) * ik.references.rightFoot.rotation;
		}

		// Auto-assign ik
		void OnDrawGizmosSelected() {
			if (ik == null) ik = GetComponent<VRIK>();
			if (ik == null) ik = GetComponentInParent<VRIK>();
			if (ik == null) ik = GetComponentInChildren<VRIK>();
		}

		// Cleaning up the delegate
		void OnDestroy() {
			if (initiated && ik != null) {
				ik.solver.OnPreUpdate -= OnSolverUpdate;
				ik.solver.OnPostUpdate -= OnPostSolverUpdate;
			}
		}
	}
}
