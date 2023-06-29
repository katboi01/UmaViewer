using UnityEngine;
using System.Collections;

	namespace RootMotion.FinalIK {
		
	/// <summary>
	/// Branch of FABRIK components in the FABRIKRoot hierarchy.
	/// </summary>
	[System.Serializable]
	public class FABRIKChain {
		
		#region Main Interface
		
		/// <summary>
		/// The FABRIK component.
		/// </summary>
		public FABRIK ik;
		/// <summary>
		/// Parent pull weight.
		/// </summary>
		[Range(0f, 1f)]
		public float pull = 1f;
		/// <summary>
		/// Resistance to being pulled by child chains.
		/// </summary>
		[Range(0f, 1f)]
		public float pin = 1f;
		/// <summary>
		/// The child chain indexes.
		/// </summary>
		public int[] children = new int[0];
		
		/// <summary>
		/// Checks whether this FABRIKChain is valid.
		/// </summary>
		public bool IsValid(ref string message) {
			if (ik == null) {
				message = "IK unassigned in FABRIKChain.";
				return false;
			}
			
			if (!ik.solver.IsValid(ref message)) return false;

			return true;
		}

		#endregion Main Interface
		
		/*
		 * Initiate the chain
		 * */
		public void Initiate() {
			ik.enabled = false;
		}
	
		/*
		 * Solving stage 1 of the FABRIK algorithm from end effectors towards the root.
		 * */
		public void Stage1(FABRIKChain[] chain) {
			// Solving children first
			for (int i = 0; i < children.Length; i++) chain[children[i]].Stage1(chain);
			
			// The last chains
			if (children.Length == 0) {
				ik.solver.SolveForward(ik.solver.GetIKPosition());
				return;
			}

			ik.solver.SolveForward(GetCentroid(chain));
		}

		/*
		 * Solving stage 2 of the FABRIK algoright from the root to the end effectors.
		 * */
		public void Stage2(Vector3 rootPosition, FABRIKChain[] chain) {
			// Solve this chain backwards
			ik.solver.SolveBackward(rootPosition);
			
			// Solve child chains
			for (int i = 0; i < children.Length; i++) {
				chain[children[i]].Stage2(ik.solver.bones[ik.solver.bones.Length - 1].transform.position, chain);
			}
		}

		// Calculate the centroid of child positions
		private Vector3 GetCentroid(FABRIKChain[] chain) {
			Vector3 position = ik.solver.GetIKPosition();
			
			// The chain is pinned, ignore the children
			if (pin >= 1f) return position;
			
			// Get the sum of the pull values of all the children
			float pullSum = 0f;
			for (int i = 0; i < children.Length; i++) pullSum += chain[children[i]].pull;
			
			// All pull values are zero
			if (pullSum <= 0f) return position;
			
			if (pullSum < 1f) pullSum = 1f;
			
			// Calculating the centroid
			Vector3 centroid = position;
			
			for (int i = 0; i < children.Length; i++) {
				// Vector from IKPosition to the first bone of the child
				Vector3 toChild = chain[children[i]].ik.solver.bones[0].solverPosition - position;

				// Weight of the child
				float childWeight = chain[children[i]].pull / pullSum;

				// Adding to the centroid
				centroid += toChild * childWeight;
			}
			
			// No pinning
			if (pin <= 0f) return centroid;
			
			// Pinning
			return centroid + (position - centroid) * pin; 
		}
	}
}
