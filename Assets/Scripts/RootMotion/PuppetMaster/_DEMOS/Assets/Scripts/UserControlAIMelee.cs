using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {
	
	/// <summary>
	/// User input for an AI controlled character controller.
	/// </summary>
	public class UserControlAIMelee : UserControlThirdPerson {

		public BehaviourPuppet targetPuppet;
		public float stoppingDistance = 0.5f;
		public float stoppingThreshold = 1.5f;

		private bool isAttacking;
		private float attackTimer;
		private Transform moveTarget { get { return targetPuppet.puppetMaster.muscles[0].joint.transform; }}

		protected override void Update () {
			// Moving
			float moveSpeed = walkByDefault? 0.5f: 1f;
			
			Vector3 direction = moveTarget.position - transform.position;
			direction.y = 0f;
			float sD = state.move != Vector3.zero? stoppingDistance: stoppingDistance * stoppingThreshold;
			
			state.move = direction.magnitude > sD? direction.normalized * moveSpeed: Vector3.zero;

			// Rotating
			state.lookPos = moveTarget.position + transform.right * -0.2f;

			// Attacking
			if (CanAttack()) attackTimer += Time.deltaTime;
			else attackTimer = 0f;

			state.actionIndex = attackTimer > 0.5f? 1: 0;
		}

		private bool CanAttack() {
			if (targetPuppet.state == BehaviourPuppet.State.Unpinned) return false;

			// Angle between forward and target direction
			Vector3 dir = state.lookPos - transform.position;
			dir = Quaternion.Inverse(transform.rotation) * dir;
			if (Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg > 20f )return false;

			return state.move == Vector3.zero;
		}
	}
}
