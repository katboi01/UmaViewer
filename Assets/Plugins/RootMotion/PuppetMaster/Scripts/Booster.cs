using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Booster for BehaviourPuppet. Can be used to enhance puppet collision resistance and/or dealing damage to other puppets.
	/// </summary>
	[System.Serializable]
	public class Booster {

		[Tooltip("If true, all the muscles will be boosted and the 'Muscles' and 'Groups' properties below will be ignored.")]
		/// <summary>
		/// If true, all the muscles will be boosted and the 'Muscles' and 'Groups' properties below will be ignored.
		/// </summary>
		public bool fullBody;

		[Tooltip("Muscles to boost. Used only when 'Full Body' is false.")]
		/// <summary>
		/// Muscles to boost. Used only when 'Full Body' is false.
		/// </summary>
		public ConfigurableJoint[] muscles = new ConfigurableJoint[0];

		[Tooltip("Muscle groups to boost. Used only when 'Full Body' is false.")]
		/// <summary>
		/// Muscle groups to boost. Used only when 'Full Body' is false.
		/// </summary>
		public Muscle.Group[] groups = new Muscle.Group[0];

		[Tooltip("Immunity to apply to the muscles. If muscle immunity is 1, it can not be damaged.")]
		/// <summary>
		/// Immunity to apply to the muscles. If muscle immunity is 1, it can not be damaged.
		/// </summary>
		[Range(0f, 1f)] public float immunity;

		[Tooltip("Impulse multiplier to be applied to the muscles. This makes them deal more damage to other puppets.")]
		/// <summary>
		/// Impulse multiplier to be applied to the muscles. This makes them deal more damage to other puppets.
		/// </summary>
		public float impulseMlp;

		[Tooltip("Falloff for parent muscles (power of kinship degree).")]
		/// <summary>
		/// Falloff for parent muscles (power of kinship degree).
		/// </summary>
		public float boostParents;

		[Tooltip("Falloff for child muscles (power of kinship degree).")]
		/// <summary>
		/// Falloff for child muscles (power of kinship degree).
		/// </summary>
		public float boostChildren;

		[Tooltip("This does nothing on it's own, you can use it in a 'yield return new WaitForseconds(delay);' call.")]
		/// <summary>
		/// This does nothing on it's own, you can use it in a 'yield return new WaitForseconds(delay);' call.
		/// </summary>
		public float delay;

		/// <summary>
		/// Boost the puppet's performance.
		/// </summary>
		public void Boost(BehaviourPuppet puppet) {
			if (fullBody) puppet.Boost(immunity, impulseMlp);
			else {
				// Muscles
				foreach (ConfigurableJoint joint in muscles) {
					for (int i = 0; i < puppet.puppetMaster.muscles.Length; i++) {
						if (puppet.puppetMaster.muscles[i].joint == joint) {
							puppet.Boost(i, immunity, impulseMlp, boostParents, boostChildren);
							break;
						}
					}
				}

				// Groups
				foreach (Muscle.Group group in groups) {
					for (int i = 0; i < puppet.puppetMaster.muscles.Length; i++) {
						if (puppet.puppetMaster.muscles[i].props.group == group) {
							puppet.Boost(i, immunity, impulseMlp, boostParents, boostChildren);
						}
					}
				}
			}
		}
	}
}
