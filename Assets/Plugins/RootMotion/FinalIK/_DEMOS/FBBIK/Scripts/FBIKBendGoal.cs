using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Bend goal for FullBodyBipedIK.
	/// </summary>
	public class FBIKBendGoal: MonoBehaviour {
		
		public FullBodyBipedIK ik; // Refernce to the FBBIK component
		public FullBodyBipedChain chain; // Which limb is this bend goal for?
		
		public float weight; // Bend goal weight

		void Start() {
			Debug.Log("FBIKBendGoal is deprecated, you can now a bend goal from the custom inspector of the FullBodyBipedIK component.");
		}

		void Update() {
			if (ik == null) return;

			ik.solver.GetBendConstraint(chain).bendGoal = transform;
			ik.solver.GetBendConstraint(chain).weight = weight;
		}
	}
}
	
	
