using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Base abstract class for %IK solver components.
	/// </summary>
	public abstract class IK: SolverManager {
		
		#region Main Interface
		
		/// <summary>
		/// Gets the %IK component's solver as IKSolver.
		/// </summary>
		public abstract IKSolver GetIKSolver();
		
		#endregion Main Interface
		
		/*
		 * Updates the solver. If you need full control of the execution order of your IK solvers, disable this script and call UpdateSolver() instead.
		 * */
		protected override void UpdateSolver() {
			if (!GetIKSolver().initiated) InitiateSolver();
			if (!GetIKSolver().initiated) return;

			GetIKSolver().Update();
		}
		
		/*
		 * Initiates the %IK solver
		 * */
		protected override void InitiateSolver() {
			if (GetIKSolver().initiated) return;
			
			GetIKSolver().Initiate(transform);
		}

		protected override void FixTransforms() {
			if (!GetIKSolver().initiated) return;
			GetIKSolver().FixTransforms();
		}

		// Open the User Manual url
		protected abstract void OpenUserManual();

		// Open the Script Reference url
		protected abstract void OpenScriptReference();
	}
}
