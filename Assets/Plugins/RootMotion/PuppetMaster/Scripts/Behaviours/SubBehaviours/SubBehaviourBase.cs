using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// The sub-behaviours take care of behaviour code reusability.
	/// While there can be only one active Puppet Behaviour at a time, that active behaviour can use multiple independent and reusable sub-behaviours simultaneously.
	/// For example the SubBehaviourCOM is responsible for calculating everything about the center of mass and can be used by any behaviour or even other sub-behaviours that need CoM calculations.
	/// This is the base abstract class for all sub-behaviours.
	/// </summary>
	[System.Serializable]
	public abstract class SubBehaviourBase {

		protected BehaviourBase behaviour;

		protected static Vector2 XZ(Vector3 v) {
			return new Vector2(v.x, v.z);
		}
		
		protected static Vector3 XYZ(Vector2 v) {
			return new Vector3(v.x, 0f, v.y);
		}
		
		protected static Vector3 Flatten(Vector3 v) {
			return new Vector3(v.x, 0f, v.z);
		}
		
		protected static Vector3 SetY(Vector3 v, float y) {
			return new Vector3(v.x, y, v.z);
		}
	}
}
