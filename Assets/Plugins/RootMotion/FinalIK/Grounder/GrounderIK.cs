using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Grounding for LimbIK, CCD and/or FABRIK solvers.
	/// </summary>
	[HelpURL("http://www.root-motion.com/finalikdox/html/page9.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder IK")]
	public class GrounderIK: Grounder {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_i_k.html");
		}

		#region Main Interface
		
		/// <summary>
		/// The leg %IK componets (can be any type of IK component).
		/// </summary>
		public IK[] legs;
		/// <summary>
		/// The pelvis transform. Common ancestor of all the legs.
		/// </summary>
		[Tooltip("The pelvis transform. Common ancestor of all the legs.")]
		public Transform pelvis;
		/// <summary>
		/// The root Transform of the character, with the rigidbody and the collider.
		/// </summary>
		[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
		public Transform characterRoot;
		/// <summary>
		/// The weight of rotating the character root to the ground normal (range: 0 - 1).
		/// </summary>
		[Tooltip("The weight of rotating the character root to the ground normal (range: 0 - 1).")]
		[Range(0f, 1f)]
		public float rootRotationWeight;
		/// <summary>
		/// The speed of rotating the character root to the ground normal (range: 0 - inf).
		/// </summary>
		[Tooltip("The speed of rotating the character root to the ground normal (range: 0 - inf).")]
		public float rootRotationSpeed = 5f;
		/// <summary>
		/// The maximum angle of root rotation (range: 0 - 90).
		/// </summary>
		[Tooltip("The maximum angle of root rotation (range: 0 - 90).")]
		public float maxRootRotationAngle = 45f;
		
		#endregion Main Interface

		public override void ResetPosition() {
			solver.Reset();
		}

		private Transform[] feet = new Transform[0];
		private Quaternion[] footRotations = new Quaternion[0];
		private Vector3 animatedPelvisLocalPosition, solvedPelvisLocalPosition;
		private int solvedFeet;
		private bool solved;
		private float lastWeight;
		private Rigidbody characterRootRigidbody;

		// Can we initiate the Grounding?
		private bool IsReadyToInitiate() {
			if (pelvis == null) return false;
			
			if (legs.Length == 0) return false;
			
			foreach (IK leg in legs) {
				if (leg == null) return false;
				
				if (leg is FullBodyBipedIK) {
					LogWarning("GrounderIK does not support FullBodyBipedIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead. If you want to use FullBodyBipedIK, use the GrounderFBBIK component.");
					return false;
				}
				
				if (leg is FABRIKRoot) {
					LogWarning("GrounderIK does not support FABRIKRoot, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
				
				if (leg is AimIK) {
					LogWarning("GrounderIK does not support AimIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
			}
			
			return true;
		}

		// Weigh out the IK solvers properly when the component is disabled
		void OnDisable() {
			if (!initiated) return;

			for (int i = 0; i < legs.Length; i++) {
				if (legs[i] != null) legs[i].GetIKSolver().IKPositionWeight = 0f;
			}
		}

		// Initiate once we have all the required components
		void Update() {
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (weight <= 0f) return;

			solved = false;

			if (initiated) {
				// Clamping values
				rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
				rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);

				// Root rotation
				if (characterRoot != null && rootRotationSpeed > 0f && rootRotationWeight > 0f) {
					Vector3 normal = solver.GetLegsPlaneNormal();
                    
					// Root rotation weight
					if (rootRotationWeight < 1f) {
						normal = Vector3.Slerp(Vector3.up, normal, rootRotationWeight);
					}

					// Root rotation limit
					Quaternion upRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * characterRoot.rotation;
					Quaternion rotationTarget = Quaternion.RotateTowards(upRotation, Quaternion.FromToRotation(transform.up, normal) * characterRoot.rotation, maxRootRotationAngle);

					// Rotate the root
					if (characterRootRigidbody == null) {
						characterRoot.rotation = Quaternion.Lerp(characterRoot.rotation, rotationTarget, Time.deltaTime * rootRotationSpeed);
					} else {
						characterRootRigidbody.MoveRotation(Quaternion.Lerp(characterRoot.rotation, rotationTarget, Time.deltaTime * rootRotationSpeed));
					}
				}

				return;
			}

			if (!IsReadyToInitiate()) return;
			
			Initiate();
		}

		private void Initiate() {
			// Building arrays
			feet = new Transform[legs.Length];
			footRotations = new Quaternion[legs.Length];

			for (int i = 0; i < feet.Length; i++) footRotations[i] = Quaternion.identity;

			// Gathering the last bones of the IK solvers as feet
			for (int i = 0; i < legs.Length; i++) {
				IKSolver.Point[] points = legs[i].GetIKSolver().GetPoints();

				feet[i] = points[points.Length - 1].transform;

				// Add to the update delegates of each ik solver
				legs[i].GetIKSolver().OnPreUpdate += OnSolverUpdate;
				legs[i].GetIKSolver().OnPostUpdate += OnPostSolverUpdate;
			}

			// Store the default localPosition of the pelvis
			animatedPelvisLocalPosition = pelvis.localPosition;

			// Initiate the Grounding
			solver.Initiate(transform, feet);

			for (int i = 0; i < legs.Length; i++) {
				if (legs [i] is LegIK) {
					solver.legs[i].invertFootCenter = true;
				}
			}

			characterRootRigidbody = characterRoot.GetComponent<Rigidbody>();
			
			initiated = true;
		}

		// Called before updating the first IK solver
		private void OnSolverUpdate() {
			if (!enabled) return;

			if (weight <= 0f) {
				if (lastWeight <= 0f) return;
				
				// Weigh out the limb solvers properly
				OnDisable();
			}
			
			lastWeight = weight;

			// If another IK has already solved in this frame, do nothing
			if (solved) return;

			if (OnPreGrounder != null) OnPreGrounder();

			// If the pelvis local position has not changed since last solved state, consider it unanimated
			if (pelvis.localPosition != solvedPelvisLocalPosition) animatedPelvisLocalPosition = pelvis.localPosition;
			else pelvis.localPosition = animatedPelvisLocalPosition;

			// Update the Grounding
			solver.Update();

			// Update the IKPositions and IKPositonWeights of the legs
			for (int i = 0; i < legs.Length; i++) SetLegIK(i);

			// Move the pelvis
			pelvis.position += solver.pelvis.IKOffset * weight;

			solved = true;
			solvedFeet = 0;

			if (OnPostGrounder != null) OnPostGrounder();
		}

		// Set the IK position and weight for a limb
		private void SetLegIK(int index) {
			footRotations[index] = feet[index].rotation;

			if (legs [index] is LegIK) {
				(legs[index].GetIKSolver() as IKSolverLeg).IKRotation = Quaternion.Slerp(Quaternion.identity, solver.legs[index].rotationOffset, weight) * footRotations[index];
				(legs[index].GetIKSolver() as IKSolverLeg).IKRotationWeight = 1f;
			}

			legs[index].GetIKSolver().IKPosition = solver.legs[index].IKPosition;
			legs[index].GetIKSolver().IKPositionWeight = weight;
		}

		// Rotating the feet after IK has finished
		private void OnPostSolverUpdate() {
			if (weight <= 0f) return;
			if (!enabled) return;

			// Only do this after the last IK solver has finished
			solvedFeet ++;
			if (solvedFeet < feet.Length) return;
            solved = false;

			for (int i = 0; i < feet.Length; i++) {
				feet[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) * footRotations[i];
			}

			// Store the local position of the pelvis so we know it it changes
			solvedPelvisLocalPosition = pelvis.localPosition;
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (initiated) {
				foreach (IK leg in legs) {
					if (leg != null) {
						leg.GetIKSolver().OnPreUpdate -= OnSolverUpdate;
						leg.GetIKSolver().OnPostUpdate -= OnPostSolverUpdate;
					}
				}
			}
		}
	}
}
