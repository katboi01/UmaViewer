using UnityEngine;
using System.Collections;

namespace RootMotion {

	/// <summary>
	/// Manages solver initiation and updating
	/// </summary>
	public class SolverManager: MonoBehaviour {
		
		#region Main Interface
		
		/// <summary>
		/// If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance. Not recommended for CCD and FABRIK solvers.
		/// </summary>
		[Tooltip("If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance. Not recommended for CCD and FABRIK solvers.")]
		public bool fixTransforms = true;

		/// <summary>
		/// [DEPRECATED] Use "enabled = false" instead.
		/// </summary>
		public void Disable() {
			Debug.Log("IK.Disable() is deprecated. Use enabled = false instead", transform);

			enabled = false;
		}

		#endregion Main

		protected virtual void InitiateSolver() {}
		protected virtual void UpdateSolver() {}
		protected virtual void FixTransforms() {}
		
		private Animator animator;
		private Animation legacy;
		private bool updateFrame;
		private bool componentInitiated;

		void OnDisable() {
			if (!Application.isPlaying) return;
			Initiate();
		}

		void Start() {
			Initiate();
		}

		private bool animatePhysics {
			get {
				if (animator != null) return animator.updateMode == AnimatorUpdateMode.AnimatePhysics;
				if (legacy != null) return legacy.animatePhysics;
				return false;
			}
		}

		private void Initiate() {
			if (componentInitiated) return;

			FindAnimatorRecursive(transform, true);
			
			InitiateSolver();
			componentInitiated = true;
		}

		void Update() {
			if (skipSolverUpdate) return;
			if (animatePhysics) return;

			if (fixTransforms) FixTransforms();
		}

		// Finds the first Animator/Animation up the hierarchy
		private void FindAnimatorRecursive(Transform t, bool findInChildren) {
			if (isAnimated) return;

			animator = t.GetComponent<Animator>();
			legacy = t.GetComponent<Animation>();

			if (isAnimated) return;

			if (animator == null && findInChildren) animator = t.GetComponentInChildren<Animator>();
			if (legacy == null && findInChildren) legacy = t.GetComponentInChildren<Animation>();

			if (!isAnimated && t.parent != null) FindAnimatorRecursive(t.parent, false);
		}

		private bool isAnimated {
			get {
				return animator != null || legacy != null;
			}
		}

		// Workaround hack for the solver to work with animatePhysics
		void FixedUpdate() {
			if (skipSolverUpdate) {
				skipSolverUpdate = false;
			}

			updateFrame = true;

			if (animatePhysics && fixTransforms) FixTransforms();
		}

		// Updating
		void LateUpdate() {
			if (skipSolverUpdate) return;

			// Check if either animatePhysics is false or FixedUpdate has been called
			if (!animatePhysics) updateFrame = true;
			if (!updateFrame) return;
			updateFrame = false;

			UpdateSolver();
		}

		// This enables other scripts to update the Animator on in FixedUpdate and the solvers with it
		private bool skipSolverUpdate;

		public void UpdateSolverExternal() {
			if (!enabled) return;

			skipSolverUpdate = true;
			
			UpdateSolver();
		}
	}
}
