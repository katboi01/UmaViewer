using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {
	
	/// <summary>
	/// Contols animation for a third person person controller for PuppetMaster "Melee" demo.
	/// </summary>
	public class CharacterAnimationMeleeDemo: CharacterAnimationThirdPerson {

		CharacterMeleeDemo melee { get { return characterController as CharacterMeleeDemo; }}

		// Update the Animator with the current state of the character controller
		protected override void Update() {
			base.Update();

			animator.SetInteger("ActionIndex", -1);

			if (melee.currentAction != null) {
				animator.SetInteger("ActionIndex", melee.currentActionIndex);

				var anim = melee.currentAction.anim;

				animator.CrossFadeInFixedTime(anim.stateName, anim.transitionDuration, anim.layer, anim.fixedTime);

				melee.currentActionIndex = -1;
			}
		}
	}
}
