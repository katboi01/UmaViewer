using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Ragdoll Utility controls switching characters in and out of ragdoll mode. It also enables you to use IK effects on top of ragdoll simulation.
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class RagdollUtility : MonoBehaviour {

		#region Main Interface

		[Tooltip("If you have multiple IK components, then this should be the one that solves last each frame.")] 
		/// <summary>
		/// If you have multiple IK components, then this should be the one that solves last each frame.
		/// </summary>
		public IK ik;

		[Tooltip("How long does it take to blend from ragdoll to animation?")]
		/// <summary>
		/// How long does it take to blend from ragdoll to animation?
		/// </summary>
		public float ragdollToAnimationTime = 0.2f;

		[Tooltip("If true, IK can be used on top of physical ragdoll simulation.")]
		/// <summary>
		/// If true, IK can be used on top of physical ragdoll simulation.
		/// </summary>
		public bool applyIkOnRagdoll;

		[Tooltip("How much velocity transfer from animation to ragdoll?")]
		/// <summary>
		/// How much velocity transfer from animation to ragdoll?
		/// </summary>
		public float applyVelocity = 1f;

		[Tooltip("How much angular velocity to transfer from animation to ragdoll?")]
		/// <summary>
		/// How much angular velocity to transfer from animation to ragdoll?
		/// </summary>
		public float applyAngularVelocity = 1f;

		/// <summary>
		/// Switches to ragdoll.
		/// </summary>
		public void EnableRagdoll() {
			if (isRagdoll) return;
			
			StopAllCoroutines();
			enableRagdollFlag = true;
		}

		/// <summary>
		/// Blends back to animation.
		/// </summary>
		public void DisableRagdoll() {
			if (!isRagdoll) return;
			StoreLocalState();
			StopAllCoroutines();
			StartCoroutine(DisableRagdollSmooth());
		}

		#endregion Main Interface

		// The rigidbodies and their associates
		public class Rigidbone {
			public Rigidbody r;
			public Transform t;
			public Collider collider;
			public Joint joint;
			public Rigidbody c;
			public bool updateAnchor;
			public Vector3 deltaPosition;
			public Quaternion deltaRotation;
			public float deltaTime;
			public Vector3 lastPosition;
			public Quaternion lastRotation;

			// Constructor
			public Rigidbone (Rigidbody r) {
				this.r = r;
				t = r.transform;
				joint = t.GetComponent<Joint>();

				collider = t.GetComponent<Collider>();

				if (joint != null) {
					c = joint.connectedBody;
					updateAnchor = c != null;
				}
				
				lastPosition = t.position;
				lastRotation = t.rotation;
			}

			// Store position and rotation deltas
			public void RecordVelocity() {
				deltaPosition = t.position - lastPosition;
				lastPosition = t.position;

				deltaRotation = RootMotion.QuaTools.FromToRotation(lastRotation, t.rotation);
				lastRotation = t.rotation;

				deltaTime = Time.deltaTime;
			}

			// Go to ragdoll
			public void WakeUp(float velocityWeight, float angularVelocityWeight) {
				// Joint anchors need to be updated when there are animated bones in between ragdoll bones
				if (updateAnchor) {
					joint.connectedAnchor = t.InverseTransformPoint(c.position);
				}

				r.isKinematic = false;

				// Transfer velocity from animation
				if (velocityWeight != 0f) {
					r.velocity = (deltaPosition / deltaTime) * velocityWeight;
				}

				// Transfer angular velocity from animation
				if (angularVelocityWeight != 0f) {
					float angle = 0f;
					Vector3 axis = Vector3.zero;
					deltaRotation.ToAngleAxis(out angle, out axis);
					angle *= Mathf.Deg2Rad;
					angle /= deltaTime;
					axis *= angle * angularVelocityWeight;
					r.angularVelocity = Vector3.ClampMagnitude(axis, r.maxAngularVelocity);
				}

				r.WakeUp();
			}
		}

		// All child Transforms of the root.
		public class Child {
			public Transform t;

			public Vector3 localPosition;
			public Quaternion localRotation;

			// Constructor
			public Child(Transform transform) {
				t = transform;
				localPosition = t.localPosition;
				localRotation = t.localRotation;
			}

			// Force to the last stored local state
			public void FixTransform(float weight) {
				if (weight <= 0f) return;
				
				if (weight >= 1f) {
					t.localPosition = localPosition;
					t.localRotation = localRotation;
					return;
				}
				
				t.localPosition = Vector3.Lerp(t.localPosition, localPosition, weight);
				t.localRotation = Quaternion.Lerp(t.localRotation, localRotation, weight);
			}

			// Remember the local state, that is the local position and rotation of the transform
			public void StoreLocalState() {
				localPosition = t.localPosition;
				localRotation = t.localRotation;
			}
		}

		private Animator animator;
		private Rigidbone[] rigidbones = new Rigidbone[0];
		private Child[] children = new Child[0];
		private bool enableRagdollFlag;
		private AnimatorUpdateMode animatorUpdateMode;
		private IK[] allIKComponents = new IK[0];
		private bool[] fixTransforms = new bool[0];
		private float ragdollWeight;
		private float ragdollWeightV;
		private bool fixedFrame;
		private bool[] disabledIKComponents = new bool[0];

		// Find all necessary components and initiate
		public void Start() {
			animator = GetComponent<Animator>();

			allIKComponents = (IK[])GetComponentsInChildren<IK>();
			disabledIKComponents = new bool[allIKComponents.Length];
			fixTransforms = new bool[allIKComponents.Length];

			if (ik != null) ik.GetIKSolver().OnPostUpdate += AfterLastIK;

			// Gather all the rigidbodies and their associates
			Rigidbody[] rigidbodies = (Rigidbody[])GetComponentsInChildren<Rigidbody>();
			int firstIndex = rigidbodies[0].gameObject == gameObject? 1: 0;

			rigidbones = new Rigidbone[firstIndex == 0? rigidbodies.Length: rigidbodies.Length - 1];

			for (int i = 0; i < rigidbones.Length; i++) {
				rigidbones[i] = new Rigidbone(rigidbodies[i + firstIndex]);
			}

			// Find all the child Transforms
			Transform[] C = (Transform[])GetComponentsInChildren<Transform>();
			children = new Child[C.Length - 1];

			for (int i = 0; i < children.Length; i++) {
				children[i] = new Child(C[i + 1]);
			}
		}

		// Smoothly blends out of Ragdoll
		private IEnumerator DisableRagdollSmooth() {
			// ...make all rigidbodies kinematic
			for (int i = 0; i < rigidbones.Length; i++) {
				rigidbones[i].r.isKinematic = true;
			}

			// Reset IK components
			for (int i = 0; i < allIKComponents.Length; i++) {
				allIKComponents[i].fixTransforms = fixTransforms[i];
				if (disabledIKComponents[i]) allIKComponents[i].enabled = true;
			}

			// Animator has not updated yet.
			animator.updateMode = animatorUpdateMode;
			animator.enabled = true;

			// Blend back to animation
			while (ragdollWeight > 0f) {
				ragdollWeight = Mathf.SmoothDamp(ragdollWeight, 0f, ref ragdollWeightV, ragdollToAnimationTime);
				if (ragdollWeight < 0.001f) ragdollWeight = 0f;
				
				yield return null;
			}
			
			yield return null;
		}

		void Update() {
			if (!isRagdoll) return;

			// Disable IK components if applyIKOnRagdoll has been set to false while in ragdoll.
			if (!applyIkOnRagdoll) {
				bool disableIK = false;
				for (int i = 0; i < allIKComponents.Length; i++) {
					if (allIKComponents[i].enabled) {
						disableIK = true;
						break;
					}
				}

				if (disableIK) {
					for (int i = 0; i < allIKComponents.Length; i++) disabledIKComponents[i] = false;
				}

				for (int i = 0; i < allIKComponents.Length; i++) {
					if (allIKComponents[i].enabled) {
						allIKComponents[i].enabled = false;
						disabledIKComponents[i] = true;
					}
				}
			} else {
				// Enable IK components if applyIKOnRagdoll has been set to true while in ragdoll
				bool enableIK = false;
				for (int i = 0; i < allIKComponents.Length; i++) {
					if (disabledIKComponents[i]) {
						enableIK = true;
						break;
					}
				}

				if (enableIK) {
					for (int i = 0; i < allIKComponents.Length; i++) {
						if (disabledIKComponents[i]) {
							allIKComponents[i].enabled = true;
						}
					}

					for (int i = 0; i < allIKComponents.Length; i++) disabledIKComponents[i] = false;
				}
			}
		}

		void FixedUpdate() {
			// When in ragdoll, move the bones to where they were after the last physics simulation, so IK won't screw up the physics
			if (isRagdoll && applyIkOnRagdoll) FixTransforms(1f);

			fixedFrame = true;
		}

		void LateUpdate() {
			// When Mecanim has animated...
			if (animator.updateMode != AnimatorUpdateMode.AnimatePhysics || (animator.updateMode == AnimatorUpdateMode.AnimatePhysics && fixedFrame)) {
				AfterAnimation();
			}

			fixedFrame = false;

			// No IK so the final pose of the character is the current pose
			if (!ikUsed) OnFinalPose();
		}

		// Called by the last IK component after it has updated
		private void AfterLastIK() {
			// We should have the final pose of the character
			if (ikUsed) OnFinalPose();
		}

		// When animation has been applied by Mecanim
		private void AfterAnimation() {
			if (isRagdoll) {
				// If is ragdoll, no animation has been applied, but we need to remember the pose after the physics step just the same
				StoreLocalState();
			} else {
				// Blending from ragdoll to animation. When ragdollWeight is zero, nothing happens here
				FixTransforms(ragdollWeight);
			}
		}

		// When we have the final pose of the character for this frame
		private void OnFinalPose() {
			if (!isRagdoll) RecordVelocities();
			if (enableRagdollFlag) RagdollEnabler();
		}

		// Switching to ragdoll
		private void RagdollEnabler() {
			// Remember the last animated pose
			StoreLocalState();

			// Disable IK components if necessary
			for (int i = 0; i < allIKComponents.Length; i++) disabledIKComponents[i] = false;

			if (!applyIkOnRagdoll) {
				for (int i = 0; i < allIKComponents.Length; i++) {
					if (allIKComponents[i].enabled) {
						allIKComponents[i].enabled = false;
						disabledIKComponents[i] = true;
					}
				}
			}
			// Switch Animator update mode to AnimatePhysics, so IK is updated in the fixed time step
			animatorUpdateMode = animator.updateMode;
			animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

			// Disable the Animator so it won't overwrite physics
			animator.enabled = false;
			
			for (int i = 0; i < rigidbones.Length; i++) rigidbones[i].WakeUp(applyVelocity, applyAngularVelocity);

			// Remember some variables so we can revert to them when coming back from ragdoll
			for (int i = 0; i < fixTransforms.Length; i++) {
				fixTransforms[i] = allIKComponents[i].fixTransforms;
				allIKComponents[i].fixTransforms = false;
			}

			ragdollWeight = 1f;
			ragdollWeightV = 0f;

			enableRagdollFlag = false;
		}

		// Is the character currently in ragdoll mode?
		private bool isRagdoll { get { return !rigidbones[0].r.isKinematic && !animator.enabled; }}

		// Store position and rotation deltas for all the rigidbodies
		private void RecordVelocities() {
			foreach (Rigidbone r in rigidbones) r.RecordVelocity();
		}

		// Is there any IK components acting on the character?
		private bool ikUsed {
			get {
				if (ik == null) return false;
				if (ik.enabled && ik.GetIKSolver().IKPositionWeight > 0) return true;

				foreach (IK k in allIKComponents) {
					if (k.enabled && k.GetIKSolver().IKPositionWeight > 0) return true;
				}
				return false;
			}
		}

		// Stored the current pose of the character
		private void StoreLocalState() {
			foreach (Child c in children) c.StoreLocalState();
		}

		// Interpolate the character to the last stored pose (see StoreLocalState)
		private void FixTransforms(float weight) {
			foreach (Child c in children) c.FixTransform(weight);
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (ik != null) {
				ik.GetIKSolver().OnPostUpdate -= AfterLastIK;
			}
		}
	}
}
