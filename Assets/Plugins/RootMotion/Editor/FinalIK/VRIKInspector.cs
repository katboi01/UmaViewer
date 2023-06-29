using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for VRIK.
	 * */
	[CustomEditor(typeof(VRIK))]
	public class VRIKInspector : Editor {

		private VRIK script { get { return target as VRIK; }}
		private MonoScript monoScript;

		void OnEnable() {
			if (serializedObject == null) return;

			// Changing the script execution order
			if (!Application.isPlaying) {
				monoScript = MonoScript.FromMonoBehaviour(script as MonoBehaviour);
				int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (currentExecutionOrder != 9998) MonoImporter.SetExecutionOrder(monoScript, 9998);

				if (script.references.isEmpty) script.AutoDetectReferences();

				script.solver.DefaultAnimationCurves();
				script.solver.GuessHandOrientations(script.references, true);

				// TODO Set dirty
			}
		}
	}
}