using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for FABRIKRoot.
	 * */
	[CustomEditor(typeof(FABRIKRoot))]
	public class FABRIKRootInspector : IKInspector {

		private FABRIKRoot script { get { return target as FABRIKRoot; }}
		private FABRIKChain selectedChain;

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9997;
			return script;
		}
		
		protected override void AddInspector() {
			// Draw the inspector for IKSolverFABRIKRoot
			IKSolverFABRIKRootInspector.AddInspector(solver, !Application.isPlaying);

			// Warning box
			string message = string.Empty;
			if (!script.solver.IsValid(ref message)) AddWarningBox(message);
		}
		
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			IKSolverFABRIKRootInspector.AddScene(script.solver, Color.cyan, true, ref selectedChain);
		}
	}
}
