using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for AimIK.
	 * */
	[CustomEditor(typeof(AimIK))]
	public class AimIKInspector : IKInspector {
		
		private AimIK script { get { return target as AimIK; }}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9997;
			return script;
		}
		
		protected override void OnApplyModifiedProperties() {
			if (!Application.isPlaying) script.solver.Initiate(script.transform);
		}
		
		protected override void AddInspector() {
			// Draw the inspector for IKSolverAim
			IKSolverAimInspector.AddInspector(solver, !Application.isPlaying);

			// Warning box
			string message = string.Empty;
			if (!script.solver.IsValid(ref message)) AddWarningBox(message);
		}	
		
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			IKSolverAimInspector.AddScene(script.solver, new Color(1f, 0f, 0.5f, 1f), true);
		}
	}
}