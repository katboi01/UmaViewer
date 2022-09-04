using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Respawning BehaviourPuppet at a random position/rotation
	public class Respawning : MonoBehaviour {

		[Tooltip("Pooled characters will be parented to this deactivated GameObject.")] public Transform pool;
		[Tooltip("Reference to the BehaviourPuppet component of the character you wish to respawn.")] public BehaviourPuppet puppet;
		[Tooltip("The animation to play on respawn.")] public string idleAnimation;

		private bool isPooled { get { return puppet.transform.root == pool; }}
		private Transform puppetRoot;

		void Start() {
			// Store the root Transform of the puppet
			puppetRoot = puppet.transform.root;

			// Deactivate the pool so anyhting parented to it would be deactivated too
			pool.gameObject.SetActive(false);
		}

		void Update () {
			if (Input.GetKeyDown(KeyCode.Alpha1)) puppet.puppetMaster.state = PuppetMaster.State.Alive;
			if (Input.GetKeyDown(KeyCode.Alpha2)) puppet.puppetMaster.state = PuppetMaster.State.Dead;
			if (Input.GetKeyDown(KeyCode.Alpha3)) puppet.puppetMaster.state = PuppetMaster.State.Frozen;

			if (Input.GetKeyDown(KeyCode.P) && !isPooled) {
				Pool();
			}

			// Pool/Respawn from the pool
			if (Input.GetKeyDown(KeyCode.R)) {
				// Respawn in random position/rotation
				Vector2 rndCircle = UnityEngine.Random.insideUnitCircle * 2f;

				Respawn(new Vector3(rndCircle.x, 0f, rndCircle.y), Quaternion.LookRotation(new Vector3(-rndCircle.x, 0f, -rndCircle.y)));
			}
		}

		private void Pool() {
			puppetRoot.parent = pool;
		}

		private void Respawn(Vector3 position, Quaternion rotation) {
			puppet.puppetMaster.state = PuppetMaster.State.Alive;
            if (puppet.puppetMaster.targetAnimator.gameObject.activeInHierarchy) puppet.puppetMaster.targetAnimator.Play(idleAnimation, 0, 0f);
            puppet.SetState(BehaviourPuppet.State.Puppet);
			puppet.puppetMaster.Teleport(position, rotation, true);

			puppetRoot.parent = null;
		}
	}
}
