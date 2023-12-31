using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for TrigonometricIK.
	 * */
	[CustomEditor(typeof(TrigonometricIK))]
	public class TrigonometricIKInspector : IKInspector {
		
		private TrigonometricIK script { get { return target as TrigonometricIK; }}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9997;
			return script;
		}
		
		protected override void OnApplyModifiedProperties() {
			if (!Application.isPlaying) script.solver.Initiate(script.transform);
		}
		
		protected override void AddInspector() {
			// Draw the inspector for IKSolverTrigonometric
			IKSolverTrigonometricInspector.AddInspector(solver, !Application.isPlaying, true);

			// Warning box
			string message = string.Empty;
			if (!script.solver.IsValid(ref message)) AddWarningBox(message);
		}
		
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			IKSolverTrigonometricInspector.AddScene(script.solver, new Color(0f, 1f, 1f, 1f), true);
		}
	}
}
