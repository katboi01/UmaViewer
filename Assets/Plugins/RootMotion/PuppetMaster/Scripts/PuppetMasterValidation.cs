using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {
	
	// Code for making sure if the PuppetMaster setup is valid.
	public partial class PuppetMaster: MonoBehaviour {

		/// <summary>
		/// Determines whether this PuppetMaster instance is valid for initiation.
		/// </summary>
		public bool IsValid(bool log) {
			if (muscles == null) {
				if (log) Debug.LogError("PuppetMaster Muscles is null.", transform);
				return false;
			}
			
			if (muscles.Length == 0) {
				if (log) Debug.LogError("PuppetMaster has no muscles.", transform);
				return false;
			}
			
			for (int i = 0; i < muscles.Length; i++) {
				if (muscles[i] == null) {
					if (log) Debug.LogError("Muscle is null, PuppetMaster muscle setup is invalid.", transform);
					return false;
				}
				
				if (!muscles[i].IsValid(log)) return false;
			}

			if (targetRoot == null) {
				if (log) Debug.LogError("'Target Root' of PuppetMaster is null.");
				return false;
			}

			if (targetRoot.position != transform.position) {
				if (log) Debug.LogError("The position of the animated character (Target) must match with the position of the PuppetMaster when initiating PuppetMaster. If you are creating the Puppet in runtime, make sure you don't move the Target to another position immediatelly after instantiation. Move the Root Transform instead.");
				return false;
			}

			/*
			if (targetRoot.root != transform.root) {
				if (log) Debug.LogWarning("Target Root is not parented to the same root Transform as the PuppetMaster.", transform);
				return false;
			}
			*/

			if (targetRoot == null) {
				if (log) Debug.LogError("Invalid PuppetMaster setup. (targetRoot not found)", transform);
				return false;
			}

			for (int i = 0; i < muscles.Length; i++) {
				for (int c = 0; c < muscles.Length; c++) {
					if (i != c) {
						if (muscles[i] == muscles[c] || muscles[i].joint == muscles[c].joint) {
							if (log) Debug.LogError("Joint " + muscles[i].joint.name + " is used by multiple muscles (indexes " + i + " and " + c + "), PuppetMaster muscle setup is invalid.", transform);
							return false;
						}
					}
				}
			}
			
			if (muscles[0].joint.connectedBody != null && muscles.Length > 1) {
				for (int i = 1; i < muscles.Length; i++) {
					if (muscles[i].joint.GetComponent<Rigidbody>() == muscles[0].joint.connectedBody) {
						if (log) Debug.LogError("The first muscle needs to be the one that all the others are connected to (the hips).", transform);
						return false;
					}
				}
			}

			for (int i = 0; i < muscles.Length; i++) {
				if (Vector3.SqrMagnitude(muscles[i].joint.transform.position - muscles[i].target.position) > 0.001f) {
					if (log) Debug.LogError("The position of each muscle needs to match with the position of it's target. Muscle '" + muscles[i].joint.name + "' position does not match with it's target. Right-click on the PuppetMaster component's header and select 'Fix Muscle Positions' from the context menu.", muscles[i].joint.transform);
					return false;
				}
			}

			CheckMassVariation(100f, true);
			
			return true;
		}

		// Logs a warning if mass variation between the Rigidbodies in the ragdoll is more than 10 times.
		private bool CheckMassVariation(float threshold, bool log) {
			float minMass = Mathf.Infinity;
			float maxMass = 0f;
			for (int i = 0; i < muscles.Length; i++) {
				float mass = muscles[i].joint.GetComponent<Rigidbody>().mass;
				if (mass < minMass) minMass = mass;
				if (mass > maxMass) maxMass = mass;
			}

			if (maxMass / minMass > threshold) {
				if (log) {
					Debug.LogWarning("Mass variation between the Rigidbodies in the ragdoll is more than " + threshold.ToString() + " times. This might cause instability and unwanted results with Rigidbodies connected by Joints. Min mass: " + minMass + ", max mass: " + maxMass, transform);
				}
				return false;
			}

			return true;
		}

		// Log an error if API is called before initiation.
		private bool CheckIfInitiated() {
			if (!initiated) Debug.LogError("PuppetMaster has not been initiated yet.");
			return initiated;
		}
	}
}
