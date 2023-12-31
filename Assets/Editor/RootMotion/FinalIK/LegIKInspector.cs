﻿using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {
	
	/*
	 * Custom inspector for LegIK.
	 * */
	[CustomEditor(typeof(LegIK))]
	public class LegIKInspector : IKInspector {
		
		private LegIK script { get { return target as LegIK; }}
		
		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9997;
			return script;
		}
		
		protected override void OnApplyModifiedProperties() {
			if (!Application.isPlaying) script.solver.Initiate(script.transform);
		}
		
		protected override void AddInspector() {
			// Draw the inspector for IKSolverTrigonometric
			IKSolverLegInspector.AddInspector(solver, !Application.isPlaying, true);
			
			// Warning box
			string message = string.Empty;
			if (!script.solver.IsValid(ref message)) AddWarningBox(message);
		}
		
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			IKSolverLegInspector.AddScene(script.solver, new Color(0f, 1f, 1f, 1f), true);
		}
	}
}