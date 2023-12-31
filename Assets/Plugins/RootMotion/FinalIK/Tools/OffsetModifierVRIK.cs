using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Base class for all FBBIK effector positionOffset modifiers. Works with animatePhysics, safe delegates, offset limits.
	/// </summary>
	public abstract class OffsetModifierVRIK: MonoBehaviour {

		[Tooltip("The master weight")]
		public float weight = 1f;

		[Tooltip("Reference to the VRIK component")]
		public VRIK ik;

		// not using Time.deltaTime or Time.fixedDeltaTime here, because we don't know if animatePhysics is true or not on the character, so we have to keep track of time ourselves.
		protected float deltaTime { get { return Time.time - lastTime; }}
		protected abstract void OnModifyOffset();

		private float lastTime;

		protected virtual void Start() {
			StartCoroutine(Initiate());
		}
		
		private IEnumerator Initiate() {
			while (ik == null) yield return null;

			// You can use just LateUpdate, but note that it doesn't work when you have animatePhysics turned on for the character.
			ik.solver.OnPreUpdate += ModifyOffset;
			lastTime = Time.time;
		}

		// The main function that checks for all conditions and calls OnModifyOffset if they are met
		private void ModifyOffset() {
			if (!enabled) return;
			if (weight <= 0f) return;
			if (deltaTime <= 0f) return;
			if (ik == null) return;
			weight = Mathf.Clamp(weight, 0f, 1f);

			OnModifyOffset();

			lastTime = Time.time;
		}

		// Remove the delegate when destroyed
		protected virtual void OnDestroy() {
			if (ik != null) ik.solver.OnPreUpdate -= ModifyOffset;
		}
	}

}
