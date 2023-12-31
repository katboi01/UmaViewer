using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for CCDIK.
	 * */
	[CustomEditor(typeof(CCDIK))]
	public class CCDIKInspector : IKInspector {

		private CCDIK script { get { return target as CCDIK; }}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9997;
			return script;
		}

		protected override void OnApplyModifiedProperties() {
			if (!Application.isPlaying) script.solver.Initiate(script.transform);
		}
		
		protected override void AddInspector() {
			// Draw the inspector for IKSolverCCD
			IKSolverHeuristicInspector.AddInspector(solver, !Application.isPlaying, true);

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