using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// The base abstract class for all %IK solvers
	/// </summary>
	[System.Serializable]
	public abstract class IKSolver {
		
		#region Main Interface

		/// <summary>
		/// Determines whether this instance is valid or not.
		/// </summary>
		public bool IsValid() {
			string message = string.Empty;
			return IsValid(ref message);
		}

		/// <summary>
		/// Determines whether this instance is valid or not. If returns false, also fills in an error message.
		/// </summary>
		public abstract bool IsValid(ref string message);
		
		/// <summary>
		/// Initiate the solver with specified root Transform. Use only if this %IKSolver is not a member of an %IK component.
		/// </summary>
		public void Initiate(Transform root) {
			if (OnPreInitiate != null) OnPreInitiate();

			if (root == null) Debug.LogError("Initiating IKSolver with null root Transform.");
			this.root = root;
			initiated = false;

			string message = string.Empty;
			if (!IsValid(ref message)) {
				Warning.Log(message, root, false);
				return;
			}


			OnInitiate();
			StoreDefaultLocalState();
			initiated = true;
			firstInitiation = false;

			if (OnPostInitiate != null) OnPostInitiate();
		}
		
		/// <summary>
		/// Updates the %IK solver. Use only if this %IKSolver is not a member of an %IK component or the %IK component has been disabled and you intend to manually control the updating.
		/// </summary>
		public void Update() {
			if (OnPreUpdate != null) OnPreUpdate();

			if (firstInitiation) Initiate(root); // when the IK component has been disabled in Awake, this will initiate it.
			if (!initiated) return;

			OnUpdate();

			if (OnPostUpdate != null) OnPostUpdate();
		}
		
		/// <summary>
		/// The %IK position.
		/// </summary>
		[HideInInspector] public Vector3 IKPosition;

		[Tooltip("The positional or the master weight of the solver.")]
		/// <summary>
		/// The %IK position weight or the master weight of the solver.
		/// </summary>
		[Range(0f, 1f)]
		public float IKPositionWeight = 1f;
		
		/// <summary>
		/// Gets the %IK position. NOTE: You are welcome to read IKPosition directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public virtual Vector3 GetIKPosition() {
			return IKPosition;
		}
		
		/// <summary>
		/// Sets the %IK position. NOTE: You are welcome to set IKPosition directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public void SetIKPosition(Vector3 position) {
			IKPosition = position;
		}
		
		/// <summary>
		/// Gets the %IK position weight. NOTE: You are welcome to read IKPositionWeight directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public float GetIKPositionWeight() {
			return IKPositionWeight;
		}
		
		/// <summary>
		/// Sets the %IK position weight. NOTE: You are welcome to set IKPositionWeight directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public void SetIKPositionWeight(float weight) {
			IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		}
		
		/// <summary>
		/// Gets the root Transform.
		/// </summary>
		public Transform GetRoot() {
			return root;
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="IKSolver"/> has successfully initiated.
		/// </summary>
		public bool initiated { get; private set; }

		/// <summary>
		/// Gets all the points used by the solver.
		/// </summary>
		public abstract IKSolver.Point[] GetPoints();
		
		/// <summary>
		/// Gets the point with the specified Transform.
		/// </summary>
		public abstract IKSolver.Point GetPoint(Transform transform);

		/// <summary>
		/// Fixes all the Transforms used by the solver to their initial state.
		/// </summary>
		public abstract void FixTransforms();

		/// <summary>
		/// Stores the default local state for the bones used by the solver.
		/// </summary>
		public abstract void StoreDefaultLocalState();
		
		/// <summary>
		/// The most basic element type in the %IK chain that all other types extend from.
		/// </summary>
		[System.Serializable]
		public class Point {

			/// <summary>
			/// The transform.
			/// </summary>
			public Transform transform;
			/// <summary>
			/// The weight of this bone in the solver.
			/// </summary>
			[Range(0f, 1f)]
			public float weight = 1f;
			/// <summary>
			/// Virtual position in the %IK solver.
			/// </summary>
			public Vector3 solverPosition;
			/// <summary>
			/// Virtual rotation in the %IK solver.
			/// </summary>
			public Quaternion solverRotation = Quaternion.identity;
			/// <summary>
			/// The default local position of the Transform.
			/// </summary>
			public Vector3 defaultLocalPosition;
			/// <summary>
			/// The default local rotation of the Transform.
			/// </summary>
			public Quaternion defaultLocalRotation;

			/// <summary>
			/// Stores the default local state of the point.
			/// </summary>
			public void StoreDefaultLocalState() {
				defaultLocalPosition = transform.localPosition;
				defaultLocalRotation = transform.localRotation;
			}

			/// <summary>
			/// Fixes the transform to it's default local state.
			/// </summary>
			public void FixTransform() {
				if (transform.localPosition != defaultLocalPosition) transform.localPosition = defaultLocalPosition;
				if (transform.localRotation != defaultLocalRotation) transform.localRotation = defaultLocalRotation;
			}

			/// <summary>
			/// Updates the solverPosition (in world space).
			/// </summary>
			public void UpdateSolverPosition() {
				solverPosition = transform.position;
			}

			/// <summary>
			/// Updates the solverPosition (in local space).
			/// </summary>
			public void UpdateSolverLocalPosition() {
				solverPosition = transform.localPosition;
			}

			/// <summary>
			/// Updates the solverPosition/Rotation (in world space).
			/// </summary>
			public void UpdateSolverState() {
				solverPosition = transform.position;
				solverRotation = transform.rotation;
			}

			/// <summary>
			/// Updates the solverPosition/Rotation (in local space).
			/// </summary>
			public void UpdateSolverLocalState() {
				solverPosition = transform.localPosition;
				solverRotation = transform.localRotation;
			}
		}
		
		/// <summary>
		/// %Bone type of element in the %IK chain. Used in the case of skeletal Transform hierarchies.
		/// </summary>
		[System.Serializable]
		public class Bone: Point {
			
			/// <summary>
			/// The length of the bone.
			/// </summary>
			public float length;
			/// <summary>
			/// The sqr mag of the bone.
			/// </summary>
			public float sqrMag;
			/// <summary>
			/// Local axis to target/child bone.
			/// </summary>
			public Vector3 axis = -Vector3.right;
			
			/// <summary>
			/// Gets the rotation limit component from the Transform if there is any.
			/// </summary>
			public RotationLimit rotationLimit {
				get {
					if (!isLimited) return null;
					if (_rotationLimit == null) _rotationLimit = transform.GetComponent<RotationLimit>();
					isLimited = _rotationLimit != null;
					return _rotationLimit;
				}
				set {
					_rotationLimit = value;
					isLimited = value != null;
				}
			}
				
			/*
			 * Swings the Transform's axis towards the swing target
			 * */
			public void Swing(Vector3 swingTarget, float weight = 1f) {
				if (weight <= 0f) return;

				Quaternion r = Quaternion.FromToRotation(transform.rotation * axis, swingTarget - transform.position);

				if (weight >= 1f) {
					transform.rotation = r * transform.rotation;
					return;
				}

				transform.rotation = Quaternion.Lerp(Quaternion.identity, r, weight) * transform.rotation;
			}

			public static void SolverSwing(Bone[] bones, int index, Vector3 swingTarget, float weight = 1f) {
				if (weight <= 0f) return;
				
				Quaternion r = Quaternion.FromToRotation(bones[index].solverRotation * bones[index].axis, swingTarget - bones[index].solverPosition);
				
				if (weight >= 1f) {
					for (int i = index; i < bones.Length; i++) {
						bones[i].solverRotation = r * bones[i].solverRotation;
					}
					return;
				}
				
				for (int i = index; i < bones.Length; i++) {
					bones[i].solverRotation = Quaternion.Lerp(Quaternion.identity, r, weight) * bones[i].solverRotation;
				}
			}

			/*
			 * Swings the Transform's axis towards the swing target on the XY plane only
			 * */
			public void Swing2D(Vector3 swingTarget, float weight = 1f) {
				if (weight <= 0f) return;

				Vector3 from = transform.rotation * axis;
				Vector3 to = swingTarget - transform.position;

				float angleFrom = Mathf.Atan2(from.x, from.y) * Mathf.Rad2Deg;
				float angleTo = Mathf.Atan2(to.x, to.y) * Mathf.Rad2Deg;

				transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(angleFrom, angleTo) * weight, Vector3.back) * transform.rotation;
			}
			
			/*
			 * Moves the bone to the solver position
			 * */
			public void SetToSolverPosition() {
				transform.position = solverPosition;
			}
			
			public Bone() {}
			
			public Bone (Transform transform) {
				this.transform = transform;
			}
			
			public Bone (Transform transform, float weight) {
				this.transform = transform;
				this.weight = weight;
			}
			
			private RotationLimit _rotationLimit;
			private bool isLimited = true;
		}
		
		/// <summary>
		/// %Node type of element in the %IK chain. Used in the case of mixed/non-hierarchical %IK systems
		/// </summary>
		[System.Serializable]
		public class Node: Point {
			
			/// <summary>
			/// Distance to child node.
			/// </summary>
			public float length;
			/// <summary>
			/// The effector position weight.
			/// </summary>
			public float effectorPositionWeight;
			/// <summary>
			/// The effector rotation weight.
			/// </summary>
			public float effectorRotationWeight;
			/// <summary>
			/// Position offset.
			/// </summary>
			public Vector3 offset;
			
			public Node() {}
			
			public Node (Transform transform) {
				this.transform = transform;
			}
			
			public Node (Transform transform, float weight) {
				this.transform = transform;
				this.weight = weight;
			}
		}

		/// <summary>
		/// Delegates solver update events.
		/// </summary>
		public delegate void UpdateDelegate();
		/// <summary>
		/// Delegates solver iteration events.
		/// </summary>
		public delegate void IterationDelegate(int i);

		/// <summary>
		/// Called before initiating the solver.
		/// </summary>
		public UpdateDelegate OnPreInitiate;
		/// <summary>
		/// Called after initiating the solver.
		/// </summary>
		public UpdateDelegate OnPostInitiate;
		/// <summary>
		/// Called before updating.
		/// </summary>
		public UpdateDelegate OnPreUpdate;
		/// <summary>
		/// Called after writing the solved pose
		/// </summary>
		public UpdateDelegate OnPostUpdate;
		
		#endregion Main Interface
		
		protected abstract void OnInitiate();
		protected abstract void OnUpdate();

		protected bool firstInitiation = true;
		[SerializeField][HideInInspector] protected Transform root;
		
		protected void LogWarning(string message) {
			Warning.Log(message, root, true);
		}

		#region Class Methods

		/// <summary>
		/// Checks if an array of objects contains any duplicates.
		/// </summary>
		public static Transform ContainsDuplicateBone(Bone[] bones) {
			for (int i = 0; i < bones.Length; i++) {
				for (int i2 = 0; i2 < bones.Length; i2++) {
					if (i != i2 && bones[i].transform == bones[i2].transform) return bones[i].transform;
				}
			}
			return null;
		}

		/*
		 * Make sure the bones are in valid Hierarchy
		 * */
		public static bool HierarchyIsValid(IKSolver.Bone[] bones) {
			for (int i = 1; i < bones.Length; i++) {
				// If parent bone is not an ancestor of bone, the hierarchy is invalid
				if (!Hierarchy.IsAncestor(bones[i].transform, bones[i - 1].transform)) {
					return false;
				}
			}
			return true;
		}

		// Calculates bone lengths and axes, returns the length of the entire chain
		protected static float PreSolveBones(ref Bone[] bones) {
			float length = 0;
			
			for (int i = 0; i < bones.Length; i++) {
				bones[i].solverPosition = bones[i].transform.position;
				bones[i].solverRotation = bones[i].transform.rotation;
			}
			
			for (int i = 0; i < bones.Length; i++) {
				if (i < bones.Length - 1) {
					bones[i].sqrMag = (bones[i + 1].solverPosition - bones[i].solverPosition).sqrMagnitude;
					bones[i].length = Mathf.Sqrt(bones[i].sqrMag);
					length += bones[i].length;
					
					bones[i].axis = Quaternion.Inverse(bones[i].solverRotation) * (bones[i + 1].solverPosition - bones[i].solverPosition);
				} else {
					bones[i].sqrMag = 0f;
					bones[i].length = 0f;
				}
			}
			
			return length;
		}

		#endregion Class Methods
	}
}

