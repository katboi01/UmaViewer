using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Full Body %IK System designed specifically for bipeds
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=7__IafZGwvI&index=1&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Full Body Biped IK")]
	public class FullBodyBipedIK : IK {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page8.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_full_body_biped_i_k.html");
		}
		
		// Open a video tutorial about setting up the component
		[ContextMenu("TUTORIAL VIDEO (SETUP)")]
		void OpenSetupTutorial() {
			Application.OpenURL("https://www.youtube.com/watch?v=7__IafZGwvI");
		}
		
		// Open a video tutorial about the component's inspector.
		[ContextMenu("TUTORIAL VIDEO (INSPECTOR)")]
		void OpenInspectorTutorial() {
			Application.OpenURL("https://www.youtube.com/watch?v=tgRMsTphjJo");
		}

		// Link to the Final IK Google Group
		[ContextMenu("Support Group")]
		void SupportGroup() {
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}

		// Link to the Final IK Asset Store thread in the Unity Community
		[ContextMenu("Asset Store Thread")]
		void ASThread() {
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		/// <summary>
		/// The biped definition. Don't change refences directly in runtime, use SetReferences(BipedReferences references) instead.
		/// </summary>
		public BipedReferences references = new BipedReferences();
		
		/// <summary>
		/// The FullBodyBiped %IK solver.
		/// </summary>
		public IKSolverFullBodyBiped solver = new IKSolverFullBodyBiped();

		/// <summary>
		/// Sets the solver to new biped references.
		/// </summary>
		/// /// <param name="references">Biped references.</param>
		/// <param name="rootNode">Root node. if null, will try to detect the root node bone automatically. </param>
		public void SetReferences(BipedReferences references, Transform rootNode) {
			this.references = references;
			solver.SetToReferences(this.references, rootNode);
		}

		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		/// <summary>
		/// Checks the biped references for errors. Returns true if error found.
		/// </summary>
		public bool ReferencesError(ref string errorMessage) {
			// All the errors common to all bipeds
			if (BipedReferences.SetupError(references, ref errorMessage)) return true;
			
			// All the errors specific to FBBIK
			if (references.spine.Length == 0) {
				errorMessage = "References has no spine bones assigned, can not initiate the solver.";
				return true;
			}
			
			if (solver.rootNode == null) {
				errorMessage = "Root Node bone is null, can not initiate the solver.";
				return true;
			}
			
			if (solver.rootNode != references.pelvis) {
				bool inSpine = false;
				
				for (int i = 0; i < references.spine.Length; i++) {
					if (solver.rootNode == references.spine[i]) {
						inSpine = true;
						break;
					}
				}
				
				if (!inSpine) {
					errorMessage = "The Root Node has to be one of the bones in the Spine or the Pelvis, can not initiate the solver.";
					return true;
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// Check for possible warnings with the biped references setup. Returns true if warning found. The solver can still run, but probably not how you expected.
		/// </summary>
		public bool ReferencesWarning(ref string warningMessage) {
			// Check for all the warnings common to all bipeds
			if (BipedReferences.SetupWarning(references, ref warningMessage)) return true;

			// Check for warnings specific to FBBIK
			Vector3 toRightShoulder = references.rightUpperArm.position - references.leftUpperArm.position;
			Vector3 shoulderToRootNode = solver.rootNode.position - references.leftUpperArm.position;
			float dot = Vector3.Dot(toRightShoulder.normalized, shoulderToRootNode.normalized);
			
			if (dot > 0.95f) {
				warningMessage = "The root node, the left upper arm and the right upper arm bones should ideally form a triangle that is as close to equilateral as possible. " +
					"Currently the root node bone seems to be very close to the line between the left upper arm and the right upper arm bones. This might cause unwanted behaviour like the spine turning upside down when pulled by a hand effector." +
						"Please set the root node bone to be one of the lower bones in the spine.";
				return true;
			}
			
			Vector3 toRightThigh = references.rightThigh.position - references.leftThigh.position;
			Vector3 thighToRootNode = solver.rootNode.position - references.leftThigh.position;
			dot = Vector3.Dot(toRightThigh.normalized, thighToRootNode.normalized);
			
			if (dot > 0.95f) {
				warningMessage = "The root node, the left thigh and the right thigh bones should ideally form a triangle that is as close to equilateral as possible. " +
					"Currently the root node bone seems to be very close to the line between the left thigh and the right thigh bones. This might cause unwanted behaviour like the hip turning upside down when pulled by an effector." +
						"Please set the root node bone to be one of the higher bones in the spine.";
				return true;
			}

			return false;
		}

		// Reinitiates the solver to the current references
		[ContextMenu("Reinitiate")]
		void Reinitiate() {
			SetReferences(references, solver.rootNode);
		}

		// Open the User Manual URL
		[ContextMenu("Auto-detect References")]
		void AutoDetectReferences() {
			references = new BipedReferences();
			BipedReferences.AutoDetectReferences(ref references, transform, new BipedReferences.AutoDetectParams(true, false));

			solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(references);
			
			solver.SetToReferences(references, solver.rootNode);
		}
	}
}
