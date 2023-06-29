using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	/// <summary>
	/// Switching PuppetMaster.state between Alive, Dead and Frozen
	/// </summary>
	public class Killing : MonoBehaviour {

		[Tooltip("Reference to the PuppetMaster component.")]
		public PuppetMaster puppetMaster;

		[Tooltip("Settings for killing and freezing the puppet.")]
		public PuppetMaster.StateSettings stateSettings = PuppetMaster.StateSettings.Default;

		void Update () {
			// Using the state settings defined above
			if (Input.GetKeyDown(KeyCode.K)) puppetMaster.Kill(stateSettings);
			if (Input.GetKeyDown(KeyCode.F)) puppetMaster.Freeze(stateSettings);
			if (Input.GetKeyDown(KeyCode.R)) puppetMaster.Resurrect();

			// Using whatever the current state settings of the puppetMaster instance
			/*
			if (Input.GetKeyDown(KeyCode.K)) puppetMaster.state = PuppetMaster.State.Dead;
			if (Input.GetKeyDown(KeyCode.F)) puppetMaster.state = PuppetMaster.State.Frozen;
			if (Input.GetKeyDown(KeyCode.R)) puppetMaster.state = PuppetMaster.State.Alive;
			*/

			// Using default state settings
			/*
			if (Input.GetKeyDown(KeyCode.K)) puppetMaster.Kill(PuppetMaster.StateSettings.Default);
			if (Input.GetKeyDown(KeyCode.F)) puppetMaster.Freeze(PuppetMaster.StateSettings.Default);
			if (Input.GetKeyDown(KeyCode.R)) puppetMaster.Resurrect();
			*/
		}
	}
}
