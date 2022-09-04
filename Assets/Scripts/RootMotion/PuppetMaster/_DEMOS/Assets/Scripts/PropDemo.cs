using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Code example for picking up/dropping props.
	public class PropDemo : MonoBehaviour {

		[Tooltip("The Prop you wish to pick up.")] 
		public PuppetMasterProp prop;

		[Tooltip("The Prop Muscle of the left hand.")] 
		public PropMuscle propMuscleLeft;

		[Tooltip("The Prop Muscle of the right hand.")] 
		public PropMuscle propMuscleRight;

		[Tooltip("If true, the prop will be picked up when PuppetMaster initiates")]
		public bool pickUpOnAwake;

		private bool right = true;

		void Start() {
            if (pickUpOnAwake) connectTo.currentProp = prop;
		}

		void Update () {
			// Picking up
			if (Input.GetKeyDown(KeyCode.P)) {
				// Makes the prop root drop any existing props and pick up the newly assigned one.
				connectTo.currentProp = prop;
			}

			// Dropping
			if (Input.GetKeyDown(KeyCode.X)) {
				// By setting the prop root's currentProp to null, the prop connected to it will be dropped.
				connectTo.currentProp = null;
			}

			// Switching prop roots.
			if (Input.GetKeyDown(KeyCode.S)) {
				// Switch hands
				right = !right;

				// Assign the prop to the other hand
				connectTo.currentProp = prop;
			}
		}

		private PropMuscle connectTo {
			get {
				return right? propMuscleRight: propMuscleLeft;
			}
		}
	}
}
