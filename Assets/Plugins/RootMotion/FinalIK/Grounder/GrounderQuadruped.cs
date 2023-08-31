using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Grounding for LimbIK, CCD and/or FABRIK solvers.
	/// </summary>
	[HelpURL("http://www.root-motion.com/finalikdox/html/page9.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Quadruped")]
	public class GrounderQuadruped: Grounder {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_quadruped.html");
		}

		#region Main Interface
		
		/// <summary>
		/// The %Grounding solver for the forelegs.
		/// </summary>
		[Tooltip("The Grounding solver for the forelegs.")]
		public Grounding forelegSolver = new Grounding();
		/// <summary>
		/// The weight of rotating the character root to the ground angle (range: 0 - 1).
		/// </summary>
		[Tooltip("The weight of rotating the character root to the ground angle (range: 0 - 1).")]
		[Range(0f, 1f)]
		public float rootRotationWeight = 0.5f;
		/// <summary>
		/// The maximum angle of rotating the quadruped downwards (going downhill, range: -90 - 0).
		/// </summary>
		[Tooltip("The maximum angle of rotating the quadruped downwards (going downhill, range: -90 - 0).")]
		[Range(-90f, 0f)]
		public float minRootRotation = -25f;
		/// <summary>
		/// The maximum angle of rotating the quadruped upwards (going uphill, range: 0 - 90).
		/// </summary>
		[Tooltip("The maximum angle of rotating the quadruped upwards (going uphill, range: 0 - 90).")]
		[Range(0f, 90f)]
		public float maxRootRotation = 45f;
		/// <summary>
		/// The speed of interpolating the character root rotation (range: 0 - inf).
		/// </summary>
		[Tooltip("The speed of interpolating the character root rotation (range: 0 - inf).")]
		public float rootRotationSpeed = 5f;
		/// <summary>
		/// The maximum IK offset for the legs (range: 0 - inf).
		/// </summary>
		[Tooltip("The maximum IK offset for the legs (range: 0 - inf).")]
		public float maxLegOffset = 0.5f;
		/// <summary>
		/// The maximum IK offset for the forelegs (range: 0 - inf).
		/// </summary>
		[Tooltip("The maximum IK offset for the forelegs (range: 0 - inf).")]
		public float maxForeLegOffset = 0.5f;
		/// <summary>
		/// The weight of maintaining the head's rotation as it was before solving the Grounding (range: 0 - 1).
		/// </summary>
		[Tooltip("The weight of maintaining the head's rotation as it was before solving the Grounding (range: 0 - 1).")]
		[Range(0f, 1f)]
		public float maintainHeadRotationWeight = 0.5f;
		/// <summary>
		/// The root Transform of the character, with the rigidbody and the collider.
		/// </summary>
		[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
		public Transform characterRoot;
		/// <summary>
		/// The pelvis transform. Common ancestor of both legs and the spine.
		/// </summary>
		[Tooltip("The pelvis transform. Common ancestor of both legs and the spine.")]
		public Transform pelvis;
		/// <summary>
		/// The last bone in the spine that is the common parent for both forelegs.
		/// </summary>
		[Tooltip("The last bone in the spine that is the common parent for both forelegs.")]
		public Transform lastSpineBone;
		/// <summary>
		/// The head (optional, if you intend to maintain it's rotation).
		/// </summary>
		[Tooltip("The head (optional, if you intend to maintain it's rotation).")]
		public Transform head;
		/// <summary>
		/// %IK componets of the hindlegs. Can be any type of IK components.
		/// </summary>
		public IK[] legs;
		/// <summary>
		/// %IK components for the forelegs. Can be any type of IK components.
		/// </summary>
		public IK[] forelegs;
		/// <summary>
		/// When using GrounderQuadruped on a spherical object, update this vector to always point towards the center of that object.
		/// </summary>
		[HideInInspector] public Vector3 gravity = Vector3.down;
		
		#endregion Main Interface

		public override void ResetPosition() {
			solver.Reset();
			forelegSolver.Reset();
		}
		
		// Contains all the required information about a foot
		public struct Foot {
			public IKSolver solver;
			public Transform transform;
			public Quaternion rotation;
			public Grounding.Leg leg;
			
			// The custom constructor
			public Foot (IKSolver solver, Transform transform) {
				this.solver = solver;
				this.transform = transform;
				this.leg = null;
				rotation = transform.rotation;
			}
		}
		
		private Foot[] feet = new Foot[0];
		private Vector3 animatedPelvisLocalPosition;
		private Quaternion animatedPelvisLocalRotation;
		private Quaternion animatedHeadLocalRotation;
		private Vector3 solvedPelvisLocalPosition;
		private Quaternion solvedPelvisLocalRotation;
		private Quaternion solvedHeadLocalRotation;
		private int solvedFeet;
		private bool solved;
		private float angle;
		private Transform forefeetRoot;
		private Quaternion headRotation;
		private float lastWeight;
		private Rigidbody characterRootRigidbody;
		
		// Can we initiate the Grounding?
		private bool IsReadyToInitiate() {
			if (pelvis == null) return false;
			if (lastSpineBone == null) return false;
			
			if (legs.Length == 0) return false;
			if (forelegs.Length == 0) return false;
			
			if (characterRoot == null) return false;
			
			if (!IsReadyToInitiateLegs(legs)) return false;
			if (!IsReadyToInitiateLegs(forelegs)) return false;
			
			return true;
		}
		
		// Are the leg IK components valid for initiation?
		private bool IsReadyToInitiateLegs(IK[] ikComponents) {
			foreach (IK leg in ikComponents) {
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
			
			for (int i = 0; i < feet.Length; i++) {
				if (feet[i].solver != null) feet[i].solver.IKPositionWeight = 0f;
			}
		}
		
		// Initiate once we have all the required components
		void Update() {
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (weight <= 0f) return;
			
			solved = false;
			
			if (initiated) return;
			if (!IsReadyToInitiate()) return;
			
			Initiate();
		}
		
		// Initiate this Grounder
		private void Initiate() {
			// Building the feet
			feet = new Foot[legs.Length + forelegs.Length];
			
			// Gathering the last bones of the IK solvers as feet
			Transform[] footBones = InitiateFeet(legs, ref feet, 0);
			Transform[] forefootBones = InitiateFeet(forelegs, ref feet, legs.Length);
			
			// Store the default localPosition and localRotation of the pelvis
			animatedPelvisLocalPosition = pelvis.localPosition;
			animatedPelvisLocalRotation = pelvis.localRotation;
			if (head != null) animatedHeadLocalRotation = head.localRotation;
			
			forefeetRoot = new GameObject().transform;
			forefeetRoot.parent = transform;
			forefeetRoot.name = "Forefeet Root";
			
			// Initiate the Grounding
			solver.Initiate(transform, footBones);
			forelegSolver.Initiate(forefeetRoot, forefootBones);
			
			for (int i = 0; i < footBones.Length; i++) feet[i].leg = solver.legs[i];
			for (int i = 0; i < forefootBones.Length; i++) feet[i + legs.Length].leg = forelegSolver.legs[i];

			characterRootRigidbody = characterRoot.GetComponent<Rigidbody>();

			initiated = true;
		}
		
		// Initiate the feet
		private Transform[] InitiateFeet(IK[] ikComponents, ref Foot[] f, int indexOffset) {
			Transform[] bones = new Transform[ikComponents.Length];
			
			for (int i = 0; i < ikComponents.Length; i++) {
				IKSolver.Point[] points = ikComponents[i].GetIKSolver().GetPoints();
				
				f[i + indexOffset] = new Foot(ikComponents[i].GetIKSolver(), points[points.Length - 1].transform);
				bones[i] = f[i + indexOffset].transform;
				
				// Add to the update delegates of each ik solver
				f[i + indexOffset].solver.OnPreUpdate += OnSolverUpdate;
				f[i + indexOffset].solver.OnPostUpdate += OnPostSolverUpdate;
			}
			
			return bones;
		}
		
		void LateUpdate () {
			if (weight <= 0f) return;

			// Clamping values
			rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
			minRootRotation = Mathf.Clamp(minRootRotation, -90f, maxRootRotation);
			maxRootRotation = Mathf.Clamp(maxRootRotation, minRootRotation, 90f);
			rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);
			maxLegOffset = Mathf.Clamp(maxLegOffset, 0f, maxLegOffset);
			maxForeLegOffset = Mathf.Clamp(maxForeLegOffset, 0f, maxForeLegOffset);
			maintainHeadRotationWeight = Mathf.Clamp(maintainHeadRotationWeight, 0f, 1f);

			// Rotate the character root
			RootRotation();
		}

		// Rotate the character along with the terrain
		private void RootRotation() {
			if (rootRotationWeight <= 0f) return;
			if (rootRotationSpeed <= 0f) return;

			solver.rotateSolver = true;
			forelegSolver.rotateSolver = true;
			
			// Get the horizontal rotation of the character
			Vector3 tangent = characterRoot.forward;

			Vector3 normal = -gravity;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Quaternion horizontalRotation = Quaternion.LookRotation(tangent, -gravity);

			// Get the direction from root hit to forelegs root hit in the space of the horizontal character rotation
			Vector3 hitDirection = forelegSolver.rootHit.point - solver.rootHit.point;
			Vector3 hitDirectionLocal = Quaternion.Inverse(horizontalRotation) * hitDirection;

			// Get the angle between the horizontal and hit directions
			float angleTarget = Mathf.Atan2(hitDirectionLocal.y, hitDirectionLocal.z) * Mathf.Rad2Deg;
			angleTarget = Mathf.Clamp(angleTarget * rootRotationWeight, minRootRotation, maxRootRotation);
			
			// Interpolate the angle
			angle = Mathf.Lerp(angle, angleTarget, Time.deltaTime * rootRotationSpeed);

			if (characterRootRigidbody == null) {
				characterRoot.rotation = Quaternion.Slerp(characterRoot.rotation, Quaternion.AngleAxis(-angle, characterRoot.right) * horizontalRotation, weight);
			} else {
				characterRootRigidbody.MoveRotation(Quaternion.Slerp(characterRoot.rotation, Quaternion.AngleAxis(-angle, characterRoot.right) * horizontalRotation, weight));
			}
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
			
			// If the bone transforms have not changed since last solved state, consider them unanimated
			if (pelvis.localPosition != solvedPelvisLocalPosition) animatedPelvisLocalPosition = pelvis.localPosition;
			else pelvis.localPosition = animatedPelvisLocalPosition;
			
			if (pelvis.localRotation != solvedPelvisLocalRotation) animatedPelvisLocalRotation = pelvis.localRotation;
			else pelvis.localRotation = animatedPelvisLocalRotation;
			
			if (head != null) {
				if (head.localRotation != solvedHeadLocalRotation) animatedHeadLocalRotation = head.localRotation;
				else head.localRotation = animatedHeadLocalRotation;
			}
			
			for (int i = 0; i < feet.Length; i++) feet[i].rotation = feet[i].transform.rotation;
			
			// Store the head rotation so it could be maintained later
			if (head != null) headRotation = head.rotation;
			
			// Position the forefeet root to the center of forefeet
			UpdateForefeetRoot();
			
			// Update the Grounding
			solver.Update();
			forelegSolver.Update();
			
			// Move the pelvis
			pelvis.position += solver.pelvis.IKOffset * weight;
			
			// Rotate the pelvis
			Vector3 spineDirection = lastSpineBone.position - pelvis.position;
			
			Vector3 newSpinePosition = 
				lastSpineBone.position + 
					forelegSolver.root.up * Mathf.Clamp(forelegSolver.pelvis.heightOffset, Mathf.NegativeInfinity, 0f) -
					solver.root.up * solver.pelvis.heightOffset;
			
			Vector3 newDirection = newSpinePosition - pelvis.position;
			
			Quaternion f = Quaternion.FromToRotation(spineDirection, newDirection);
			pelvis.rotation = Quaternion.Slerp(Quaternion.identity, f, weight) * pelvis.rotation;
			
			// Update the IKPositions and IKPositonWeights of the legs
			for (int i = 0; i < feet.Length; i++) SetFootIK(feet[i], (i < 2? maxLegOffset: maxForeLegOffset));
			
			solved = true;
			solvedFeet = 0;

			if (OnPostGrounder != null) OnPostGrounder();
		}
		
		// Position the forefeet root to the center of forefeet
		private void UpdateForefeetRoot() {
			// Get the centroid
			Vector3 foreFeetCenter = Vector3.zero;
			
			for (int i = 0; i < forelegSolver.legs.Length; i++) {
				foreFeetCenter += forelegSolver.legs[i].transform.position;
			}
			
			foreFeetCenter /= (float)forelegs.Length;
			Vector3 dir = foreFeetCenter - transform.position;
			
			// Ortho-normalize to this Transform's rotation
			Vector3 normal = transform.up;
			Vector3 tangent = dir;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			
			// Positioning the forefeet root
			forefeetRoot.position = transform.position + tangent.normalized * dir.magnitude;
		}
		
		// Set the IK position and weight for a limb
		private void SetFootIK(Foot foot, float maxOffset) {
			Vector3 direction = foot.leg.IKPosition - foot.transform.position;
			
			foot.solver.IKPosition = foot.transform.position + Vector3.ClampMagnitude(direction, maxOffset);
			foot.solver.IKPositionWeight = weight;
		}
		
		// Rotating the feet after IK has finished
		private void OnPostSolverUpdate() {
			if (weight <= 0f) return;
			if (!enabled) return;
			
			// Only do this after the last IK solver has finished
			solvedFeet ++;
			if (solvedFeet < feet.Length) return;
			
			for (int i = 0; i < feet.Length; i++) {
				feet[i].transform.rotation = Quaternion.Slerp(Quaternion.identity, feet[i].leg.rotationOffset, weight) * feet[i].rotation;
			}
			
			if (head != null) head.rotation = Quaternion.Lerp(head.rotation, headRotation, maintainHeadRotationWeight * weight);
			
			// Store the solved transform's of the bones so we know if they are not animated
			solvedPelvisLocalPosition = pelvis.localPosition;
			solvedPelvisLocalRotation = pelvis.localRotation;
			if (head != null) solvedHeadLocalRotation = head.localRotation;
		}
		
		// Cleaning up the delegates
		void OnDestroy() {
			if (initiated) {
				DestroyLegs(legs);
				DestroyLegs(forelegs);
			}
		}
		
		// Cleaning up the delegates
		private void DestroyLegs(IK[] ikComponents) {
			foreach (IK leg in ikComponents) {
				if (leg != null) {
					leg.GetIKSolver().OnPreUpdate -= OnSolverUpdate;
					leg.GetIKSolver().OnPostUpdate -= OnPostSolverUpdate;
				}
			}
		}
	}
}

