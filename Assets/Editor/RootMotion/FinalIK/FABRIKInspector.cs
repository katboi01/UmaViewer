using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for FABRIK.
	 * */
	[CustomEditor(typeof(FABRIK))]
	public class FABRIKInspector : IKInspector {

		private FABRIK script { get { return target as FABRIK; }}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9997;
			return script;
		}
		
		protected override void OnApplyModifiedProperties() {
			if (!Application.isPlaying) script.solver.Initiate(script.transform);
		}
		
		protected override void AddInspector() {
			// Draw the inspector for IKSolverFABRIK
			IKSolverHeuristicInspector.AddInspector(solver, !Application.isPlaying, false);

			// Warning box
			string message = string.Empty;
			if (!script.solver.IsValid(ref message)) AddWarningBox(message);
		}
		
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			IKSolverHeuristicInspector.AddScene(script.solver, Color.cyan, true);
		}
	}
}
