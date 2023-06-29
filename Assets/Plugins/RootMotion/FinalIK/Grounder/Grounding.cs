using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Foot placement system.
	/// </summary>
	[System.Serializable]
	public partial class Grounding {
		
		#region Main Interface

		/// <summary>
		/// The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.
		/// </summary>
		[System.Serializable]
		public enum Quality {
			Fastest,
			Simple,
			Best
		}

		/// <summary>
		/// Layers to ground the character to. Make sure to exclude the layer of the character controller.
		/// </summary>
		[Tooltip("Layers to ground the character to. Make sure to exclude the layer of the character controller.")]
		public LayerMask layers;
		/// <summary>
		/// Max step height. Maximum vertical distance of Grounding from the root of the character.
		/// </summary>
		[Tooltip("Max step height. Maximum vertical distance of Grounding from the root of the character.")]
		public float maxStep = 0.5f;
		/// <summary>
		/// The height offset of the root.
		/// </summary>
		[Tooltip("The height offset of the root.")]
		public float heightOffset;
		/// <summary>
		/// The speed of moving the feet up/down.
		/// </summary>
		[Tooltip("The speed of moving the feet up/down.")]
		public float footSpeed = 2.5f;
		/// <summary>
		/// CapsuleCast radius. Should match approximately with the size of the feet.
		/// </summary>
		[Tooltip("CapsuleCast radius. Should match approximately with the size of the feet.")]
		public float footRadius = 0.15f;
		/// <summary>
		/// Offset of the foot center along character forward axis.
		/// </summary>
		[Tooltip("Offset of the foot center along character forward axis.")]
		[HideInInspector] public float footCenterOffset; // TODO make visible in inspector if Grounder Visualization is finished.
		/// <summary>
		/// Amount of velocity based prediction of the foot positions.
		/// </summary>
		[Tooltip("Amount of velocity based prediction of the foot positions.")]
		public float prediction = 0.05f;
		/// <summary>
		/// Weight of rotating the feet to the ground normal offset.
		/// </summary>
		[Tooltip("Weight of rotating the feet to the ground normal offset.")]
		[Range(0f, 1f)]
		public float footRotationWeight = 1f;
		/// <summary>
		/// Speed of slerping the feet to their grounded rotations.
		/// </summary>
		[Tooltip("Speed of slerping the feet to their grounded rotations.")]
		public float footRotationSpeed = 7f;
		/// <summary>
		/// Max Foot Rotation Angle, Max angular offset from the foot's rotation (Reasonable range: 0-90 degrees).
		/// </summary>
		[Tooltip("Max Foot Rotation Angle. Max angular offset from the foot's rotation.")]
		[Range(0f, 90f)]
		public float maxFootRotationAngle = 45f;
		/// <summary>
		/// If true, solver will rotate with the character root so the character can be grounded for example to spherical planets. 
		/// For performance reasons leave this off unless needed.
		/// </summary>
		[Tooltip("If true, solver will rotate with the character root so the character can be grounded for example to spherical planets. For performance reasons leave this off unless needed.")]
		public bool rotateSolver;
		/// <summary>
		/// The speed of moving the character up/down.
		/// </summary>
		[Tooltip("The speed of moving the character up/down.")]
		public float pelvisSpeed = 5f;
		/// <summary>
		/// Used for smoothing out vertical pelvis movement (range 0 - 1).
		/// </summary>
		[Tooltip("Used for smoothing out vertical pelvis movement (range 0 - 1).")]
		[Range(0f, 1f)]
		public float pelvisDamper;
		/// <summary>
		/// The weight of lowering the pelvis to the lowest foot.
		/// </summary>
		[Tooltip("The weight of lowering the pelvis to the lowest foot.")]
		public float lowerPelvisWeight = 1f;
		/// <summary>
		/// The weight of lifting the pelvis to the highest foot. This is useful when you don't want the feet to go too high relative to the body when crouching.
		/// </summary>
		[Tooltip("The weight of lifting the pelvis to the highest foot. This is useful when you don't want the feet to go too high relative to the body when crouching.")]
		public float liftPelvisWeight;
		/// <summary>
		/// The radius of the spherecast from the root that determines whether the character root is grounded.
		/// </summary>
		[Tooltip("The radius of the spherecast from the root that determines whether the character root is grounded.")]
		public float rootSphereCastRadius = 0.1f;
        /// <summary>
        /// If false, keeps the foot that is over a ledge at the root level. If true, lowers the overstepping foot and body by the 'Max Step' value.
        /// </summary>
        [Tooltip("If false, keeps the foot that is over a ledge at the root level. If true, lowers the overstepping foot and body by the 'Max Step' value.")]
        public bool overstepFallsDown = true;
		/// <summary>
		/// The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.
		/// </summary>
		[Tooltip("The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.")]
		public Quality quality = Quality.Best;

		/// <summary>
		/// The %Grounding legs.
		/// </summary>
		public Leg[] legs { get; private set; }
		/// <summary>
		/// The %Grounding pelvis.
		/// </summary>
		public Pelvis pelvis { get; private set; }
		/// <summary>
		/// Gets a value indicating whether any of the legs are grounded
		/// </summary>
		public bool isGrounded { get; private set; }
		/// <summary>
		/// The root Transform
		/// </summary>
		public Transform root { get; private set; }
		/// <summary>
		/// Ground height at the root position.
		/// </summary>
		public RaycastHit rootHit { get; private set; }
		/// <summary>
		/// Is the RaycastHit from the root grounded?
		/// </summary>
		public bool rootGrounded {
			get {
				return rootHit.distance < maxStep * 2f;
			}
		}

		/// <summary>
		/// Raycasts or sphereCasts to find the root ground point. Distance of the Ray/Sphere cast is maxDistanceMlp x maxStep. Use this instead of rootHit if the Grounder is weighed out/disabled and not updated.
		/// </summary>
		public RaycastHit GetRootHit(float maxDistanceMlp = 10f) {
			RaycastHit h = new RaycastHit();
			Vector3 _up = up;
			
			Vector3 legsCenter = Vector3.zero;
			foreach (Leg leg in legs) legsCenter += leg.transform.position;
			legsCenter /= (float)legs.Length;
			
			h.point = legsCenter - _up * maxStep * 10f;
			float distMlp = maxDistanceMlp + 1;
			h.distance = maxStep * distMlp;
			
			if (maxStep <= 0f) return h;
			
			if (quality != Quality.Best) Physics.Raycast(legsCenter + _up * maxStep, -_up, out h, maxStep * distMlp, layers, QueryTriggerInteraction.Ignore);
			else Physics.SphereCast(legsCenter + _up * maxStep, rootSphereCastRadius, -up, out h, maxStep * distMlp, layers, QueryTriggerInteraction.Ignore);
			
			return h;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Grounding"/> is valid.
		/// </summary>
		public bool IsValid(ref string errorMessage) {
			if (root == null) {
				errorMessage = "Root transform is null. Can't initiate Grounding.";
				return false;
			}
			if (legs == null) {
				errorMessage = "Grounding legs is null. Can't initiate Grounding.";
				return false;
			}
			if (pelvis == null) {
				errorMessage = "Grounding pelvis is null. Can't initiate Grounding.";
				return false;
			}
			
			if (legs.Length == 0) {
				errorMessage = "Grounding has 0 legs. Can't initiate Grounding.";
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// Initiate the %Grounding as an integrated solver by providing the root Transform, leg solvers, pelvis Transform and spine solver.
		/// </summary>
		public void Initiate(Transform root, Transform[] feet) {
			this.root = root;
			initiated = false;

			rootHit = new RaycastHit();

			// Constructing Legs
			if (legs == null) legs = new Leg[feet.Length];
			if (legs.Length != feet.Length) legs = new Leg[feet.Length];
			for (int i = 0; i < feet.Length; i++) if (legs[i] == null) legs[i] = new Leg();
			
			// Constructing pelvis
			if (pelvis == null) pelvis = new Pelvis();
			
			string errorMessage = string.Empty;
			if (!IsValid(ref errorMessage)) {
				Warning.Log(errorMessage, root, false);
				return;
			}
			
			// Initiate solvers only if application is playing
			if (Application.isPlaying) {
				for (int i = 0; i < feet.Length; i++) legs[i].Initiate(this, feet[i]);
				pelvis.Initiate(this);
				
				initiated = true;
			}
		}

		/// <summary>
		/// Updates the Grounding.
		/// </summary>
		public void Update() {
			if (!initiated) return;

			if (layers == 0) LogWarning("Grounding layers are set to nothing. Please add a ground layer.");

			maxStep = Mathf.Clamp(maxStep, 0f, maxStep);
			footRadius = Mathf.Clamp(footRadius, 0.0001f, maxStep);
			pelvisDamper = Mathf.Clamp(pelvisDamper, 0f, 1f);
			rootSphereCastRadius = Mathf.Clamp(rootSphereCastRadius, 0.0001f, rootSphereCastRadius);
			maxFootRotationAngle = Mathf.Clamp(maxFootRotationAngle, 0f, 90f);
			prediction = Mathf.Clamp(prediction, 0f, prediction);
			footSpeed = Mathf.Clamp(footSpeed, 0f, footSpeed);

			// Root hit
			rootHit = GetRootHit();

			float lowestOffset = Mathf.NegativeInfinity;
			float highestOffset = Mathf.Infinity;
			isGrounded = false;

			// Process legs
			foreach (Leg leg in legs) {
				leg.Process();

				if (leg.IKOffset > lowestOffset) lowestOffset = leg.IKOffset;
				if (leg.IKOffset < highestOffset) highestOffset = leg.IKOffset;

				if (leg.isGrounded) isGrounded = true;
			}
			
			// Precess pelvis
			pelvis.Process(-lowestOffset * lowerPelvisWeight, -highestOffset * liftPelvisWeight, isGrounded);
		}

		// Calculate the normal of the plane defined by leg positions, so we know how to rotate the body
		public Vector3 GetLegsPlaneNormal() {
			if (!initiated) return Vector3.up;

            Vector3 _up = up;
            Vector3 normal = _up;

			// Go through all the legs, rotate the normal by it's offset
			for (int i = 0; i < legs.Length; i++) {
				// Direction from the root to the leg
				Vector3 legDirection = legs[i].IKPosition - root.position;

                // Find the tangent
				Vector3 legNormal = _up;
				Vector3 legTangent = legDirection;
				Vector3.OrthoNormalize(ref legNormal, ref legTangent);
				
                // Find the rotation offset from the tangent to the direction
                Quaternion fromTo = Quaternion.FromToRotation(legTangent, legDirection);
                
                // Rotate the normal
                normal = fromTo * normal;
			}
			
			return normal;
		}

		// Set everything to 0
		public void Reset() {
			if (!Application.isPlaying) return;
			pelvis.Reset();
			foreach (Leg leg in legs) leg.Reset();
		}

		#endregion Main Interface
		
		private bool initiated;

		// Logs the warning if no other warning has beed logged in this session.
		public void LogWarning(string message) {
			Warning.Log(message, root);
		}
		
		// The up vector in solver rotation space.
		public Vector3 up {
			get {
				return (useRootRotation? root.up: Vector3.up);
			}
		}
		
		// Gets the vertical offset between two vectors in solver rotation space
		public float GetVerticalOffset(Vector3 p1, Vector3 p2) {
			if (useRootRotation) {
				Vector3 v = Quaternion.Inverse(root.rotation) * (p1 - p2);
				return v.y;
			}
			
			return p1.y - p2.y;
		}
		
		// Flattens a vector to ground plane in solver rotation space
		public Vector3 Flatten(Vector3 v) {
			if (useRootRotation) {
				Vector3 tangent = v;
				Vector3 normal = root.up;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				return Vector3.Project(v, tangent);
			}
			
			v.y = 0;
			return v;
		}
		
		// Determines whether to use root rotation as solver rotation
		private bool useRootRotation {
			get {
				if (!rotateSolver) return false;
				if (root.up == Vector3.up) return false;
				return true;
			}
		}

		public Vector3 GetFootCenterOffset() {
			return root.forward * footRadius + root.forward * footCenterOffset;
		}
	}
}


