using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// %IK system for multiple branched %FABRIK chains.
	/// </summary>
	[System.Serializable]
	public class IKSolverFABRIKRoot : IKSolver {
		
		#region Main Interface

		/// <summary>
		/// Solver iterations.
		/// </summary>
		public int iterations = 4;
		/// <summary>
		/// The weight of all chains being pinned to root position.
		/// </summary>
		[Range(0f, 1f)]
		public float rootPin = 0f;
		/// <summary>
		/// The %FABRIK chains.
		/// </summary>
		public FABRIKChain[] chains = new FABRIKChain[0];

		public override bool IsValid(ref string message) {
			if (chains.Length == 0) {
				message = "IKSolverFABRIKRoot contains no chains.";
				return false;
			}

			foreach (FABRIKChain chain in chains) {
				if (!chain.IsValid(ref message)) return false;
			}

			for (int i = 0; i < chains.Length; i++) {
				for (int c = 0; c < chains.Length; c++) {
					if (i != c && chains[i].ik == chains[c].ik) {
						message = chains[i].ik.name + " is represented more than once in IKSolverFABRIKRoot chain.";
						return false;
					}
				}
			}

			// Check the children
			for (int i = 0; i < chains.Length; i++) {
				for (int c = 0; c < chains[i].children.Length; c++) {
					int childIndex = chains[i].children[c];

					if (childIndex < 0) {
						message = chains[i].ik.name + "IKSolverFABRIKRoot chain at index " + i + " has invalid children array. Child index is < 0.";
						return false;
					}

					if (childIndex == i) {
						message = chains[i].ik.name + "IKSolverFABRIKRoot chain at index " + i + " has invalid children array. Child index is referencing to itself.";
						return false;
					}

					if (childIndex >= chains.Length) {
						message = chains[i].ik.name + "IKSolverFABRIKRoot chain at index " + i + " has invalid children array. Child index > number of chains";
						return false;
					}

					// Check if the child chain doesn't have this chain among it's children
					for (int o = 0; o < chains.Length; o++) {
						if (childIndex == o) {
							for (int n = 0; n < chains[o].children.Length; n++) {
								if (chains[o].children[n] == i) {
									message = "Circular parenting. " + chains[o].ik.name + " already has " + chains[i].ik.name + " listed as it's child.";
									return false;
								}
							}
						}
					}

					// Check for duplicates
					for (int n = 0; n < chains[i].children.Length; n++) {
						if (c != n && chains[i].children[n] == childIndex) {
							message = "Chain number " + childIndex + " is represented more than once in the children of " + chains[i].ik.name;
							return false;
						}
					}


				}
			}
			
			return true;
		}

		public override void StoreDefaultLocalState() {
			rootDefaultPosition = root.localPosition;
			for (int i = 0; i < chains.Length; i++) chains[i].ik.solver.StoreDefaultLocalState();
		}

		public override void FixTransforms() {
			if (!initiated) return;

			root.localPosition = rootDefaultPosition;
			for (int i = 0; i < chains.Length; i++) chains[i].ik.solver.FixTransforms();
		}
		
		#endregion Main Interface
		
		private bool zeroWeightApplied;
		private bool[] isRoot;
		private Vector3 rootDefaultPosition;

		protected override void OnInitiate() {
			for (int i = 0; i < chains.Length; i++) chains[i].Initiate();

			isRoot = new bool[chains.Length];
			for (int i = 0; i < chains.Length; i++) {
				isRoot[i] = IsRoot(i);
			}
		}

		// Is the chain at index a root chain (not parented by any other chains)?
		private bool IsRoot(int index) {
			for (int i = 0; i < chains.Length; i++) {
				for (int c = 0; c < chains[i].children.Length; c++) {
					if (chains[i].children[c] == index) return false;
				}
			}
			return true;
		}

		protected override void OnUpdate() {
			if (IKPositionWeight <= 0 && zeroWeightApplied) return;
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			
			// Set weight of all IK solvers
			for (int i = 0; i < chains.Length; i++) chains[i].ik.solver.IKPositionWeight = IKPositionWeight;
			
			if (IKPositionWeight <= 0) {
				zeroWeightApplied = true;
				return;
			}
			
			zeroWeightApplied = false;
			
			for (int i = 0; i < iterations; i++) {
				// Solve trees from their targets
				for (int c = 0; c < chains.Length; c++) {
					if (isRoot[c]) chains[c].Stage1(chains);

				}
				
				// Get centroid of all tree roots
				Vector3 centroid = GetCentroid();
				root.position = centroid;

				// Start all trees from the centroid
				for (int c = 0; c < chains.Length; c++) {
					if (isRoot[c]) chains[c].Stage2(centroid, chains);
				}
			}
		}
		
		public override IKSolver.Point[] GetPoints() {
			IKSolver.Point[] array = new IKSolver.Point[0];
			for (int i = 0; i < chains.Length; i++) AddPointsToArray(ref array, chains[i]);
			return array;
		}
		
		public override IKSolver.Point GetPoint(Transform transform) {
			IKSolver.Point p = null;
			for (int i = 0; i < chains.Length; i++) {
				p = chains[i].ik.solver.GetPoint(transform);
				if (p != null) return p;
			}
			
			return null;
		}
		
		private void AddPointsToArray(ref IKSolver.Point[] array, FABRIKChain chain) {
			IKSolver.Point[] chainArray = chain.ik.solver.GetPoints();
			Array.Resize(ref array, array.Length + chainArray.Length);
			
			int a = 0;
			for (int i = array.Length - chainArray.Length; i < array.Length; i++) {
				array[i] = chainArray[a];
				a++;
			}
		}
		
		/*
		 * Gets the centroid position of all chains respective of their pull weights
		 * */
		private Vector3 GetCentroid() {		
			Vector3 centroid = root.position;
			if (rootPin >= 1) return centroid;

			float pullSum = 0f;
			for (int i = 0; i < chains.Length; i++) {
				if (isRoot[i]) pullSum += chains[i].pull;
			}
			
			for (int i = 0; i < chains.Length; i++) {
				if (isRoot[i] && pullSum > 0) centroid += (chains[i].ik.solver.bones[0].solverPosition - root.position) * (chains[i].pull / Mathf.Clamp(pullSum, 1f, pullSum));
			}

			return Vector3.Lerp(centroid, root.position, rootPin);
		}
	}
}
