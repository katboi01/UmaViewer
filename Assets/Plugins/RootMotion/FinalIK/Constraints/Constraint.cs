using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// The base abstract class for all Transform constraints.
	/// </summary>
	[System.Serializable]
	public abstract class Constraint {
		
		#region Main Interface
		
		/// <summary>
		/// The transform to constrain.
		/// </summary>
		public Transform transform;
		/// <summary>
		/// %Constraint weight.
		/// </summary>
		public float weight;
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Constraint"/> is valid.
		/// </summary>
		/// <value>
		/// <c>true</c> if is valid; otherwise, <c>false</c>.
		/// </value>
		public bool isValid {
			get {
				return transform != null;
			}
		}
		
		/// <summary>
		/// Updates the constraint.
		/// </summary>
		public abstract void UpdateConstraint();
		
		#endregion Main Interface
	}
}
