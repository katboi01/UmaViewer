using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	/// <summary>
	/// Demo character controller for the Full Body FPS scene.
	/// </summary>
	[RequireComponent(typeof(FPSAiming))]
	[RequireComponent(typeof(Animator))]
	public class FPSCharacter: MonoBehaviour {

		[Range(0f, 1f)] public float walkSpeed = 0.5f;

		private float sVel;
		private Animator animator;
		private FPSAiming FPSAiming;

		void Start() {
			animator = GetComponent<Animator>();
			FPSAiming = GetComponent<FPSAiming>();
		}

		void Update() {
			// Aiming down the sight of the gun when RMB is down
			FPSAiming.sightWeight = Mathf.SmoothDamp(FPSAiming.sightWeight, (Input.GetMouseButton(1)? 1f: 0f), ref sVel, 0.1f);

			// Set to full values to optimize IK
			if (FPSAiming.sightWeight < 0.001f) FPSAiming.sightWeight = 0f;
			if (FPSAiming.sightWeight > 0.999f) FPSAiming.sightWeight = 1f;

			animator.SetFloat("Speed", walkSpeed);
		}

		void OnGUI() {
			GUI.Label(new Rect(Screen.width - 210, 10, 200, 25), "Hold RMB to aim down the sight");
		}

	}
}
