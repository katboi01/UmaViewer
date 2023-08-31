using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	public class JointBreakBroadcaster : MonoBehaviour {

		/// <summary>
		/// The PuppetMaster that this muscle belongs to.
		/// </summary>
		[SerializeField][HideInInspector] public PuppetMaster puppetMaster;
		/// <summary>
		/// The index of this muscle in the PuppetMaster.muscles array.
		/// </summary>
		[SerializeField][HideInInspector] public int muscleIndex;

		void OnJointBreak() {
			if (!enabled) return;

			puppetMaster.RemoveMuscleRecursive(puppetMaster.muscles[muscleIndex].joint, true, true, MuscleRemoveMode.Numb);
		}
	}
}
