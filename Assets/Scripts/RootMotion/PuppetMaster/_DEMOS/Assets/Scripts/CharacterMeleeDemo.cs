using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {
	
	public class CharacterMeleeDemo : CharacterPuppet {
		
		[System.Serializable]
		public class Action {

			[System.Serializable]
			public class Anim {
				public string stateName;
				public int layer;
				public float transitionDuration;
				public float fixedTime;
			}

			public string name;
			//public KeyCode keyCode;
			public int inputActionIndex = 1;
			public float duration;
			public float minFrequency;
			public Anim anim;
			public int[] requiredPropTypes;
			public Booster[] boosters;
		}

		[Header("Melee")]

		public Action[] actions;
		[HideInInspector] public int currentActionIndex = -1;
		[HideInInspector] public float lastActionTime;

		protected override void Start() {
			currentActionIndex = -1;
			lastActionTime = 0f;

			base.Start();
		}

		public Action currentAction { 
			get { 
				if (currentActionIndex < 0) return null;
				return actions[currentActionIndex]; 
			}
		}

		protected override void Update() {
			// Actions
			if (puppet.state == BehaviourPuppet.State.Puppet) {

				for (int i = 0; i < actions.Length; i++) {
					if (StartAction(actions[i])) {
						currentActionIndex = i;

						foreach (Booster booster in actions[i].boosters) {
							booster.Boost(puppet);
						}

						if (propMuscle.currentProp is PuppetMasterPropMelee) {
							(propMuscle.currentProp as PuppetMasterPropMelee).StartAction(actions[i].duration);
						}

						lastActionTime = Time.time;
						lastActionMoveMag = moveDirection.magnitude;
					}
				}
			}

			if (Time.time < lastActionTime + 0.5f) {
				moveDirection = moveDirection.normalized * Mathf.Max(moveDirection.magnitude, lastActionMoveMag);
			}

			base.Update();
		}

		float lastActionMoveMag;

		private bool StartAction(Action action) {
			if (Time.time < lastActionTime + action.minFrequency) return false;
			if (userControl.state.actionIndex != action.inputActionIndex) return false;
			//if (!Input.GetKey(action.keyCode)) return false;

			if (action.requiredPropTypes.Length > 0) {
				if (propMuscle.currentProp == null && action.requiredPropTypes[0] == -1) return true;
				if (propMuscle.currentProp == null) return false;

				bool incl = false;

				for (int i = 0; i < action.requiredPropTypes.Length; i++) {
					if (action.requiredPropTypes[i] == propMuscle.currentProp.propType) {
						incl = true;
						break;
					}
				}

				if (!incl) return false;
			}

			return true;
		}
	}


}
