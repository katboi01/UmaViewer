using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for LimbIK.
	 * */
	[CustomEditor(typeof(LimbIK))]
	public class LimbIKInspector : IKInspector {
		
		private LimbIK script { get { return target as LimbIK; }}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9997;
			return script;
		}
		
		protected override void OnApplyModifiedProperties() {
			if (!Application.isPlaying) script.solver.Initiate(script.transform);
		}
		
		protected override void AddInspector() {
			// Draw the inspector for IKSolverLimb
			IKSolverLimbInspector.AddInspector(solver, !Application.isPlaying, true);

			// Warning box
			string message = string.Empty;
			if (!script.solver.IsValid(ref message)) AddWarningBox(message);
		}	

		void OnSceneGUI() {
			// Draw the scene veiw helpers
			IKSolverLimbInspector.AddScene(script.solver, new Color(0f, 1f, 1f, 1f), true);
		}
	}
}
