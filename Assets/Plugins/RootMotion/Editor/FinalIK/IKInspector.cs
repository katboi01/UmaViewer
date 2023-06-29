using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Base abstract class for IK component inspectors.
	 * */
	public abstract class IKInspector : Editor {
		
		protected abstract void AddInspector();
		protected abstract MonoBehaviour GetMonoBehaviour(out int executionOrder);

		protected SerializedProperty solver;
		protected SerializedContent fixTransforms;
		protected SerializedContent[] content;
		protected virtual void OnApplyModifiedProperties() {}
		protected virtual void OnEnableVirtual() {}

		private MonoScript monoScript;

		void OnEnable() {
			if (serializedObject == null) return;

			// Changing the script execution order
			if (!Application.isPlaying) {
				int executionOrder = 0;
				monoScript = MonoScript.FromMonoBehaviour(GetMonoBehaviour(out executionOrder));
				int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (currentExecutionOrder != executionOrder) MonoImporter.SetExecutionOrder(monoScript, executionOrder);
			}

			solver = serializedObject.FindProperty("solver");
			fixTransforms = new SerializedContent(serializedObject.FindProperty("fixTransforms"), new GUIContent("Fix Transforms", "If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance. Not recommended for CCD and FABRIK solvers."));

			OnEnableVirtual();
		}

		// Override the default warning box
		protected virtual void AddWarningBox(string message) {
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Invalid/incomplete setup, can not initiate the solver. " + message, EditorStyles.helpBox);
			
			EditorGUILayout.Space();
		}

		#region Inspector
		
		public override void OnInspectorGUI() {
			if (serializedObject == null) return;

			serializedObject.Update();
			
			Inspector.AddContent(fixTransforms);
			
			AddInspector();

			if (serializedObject.ApplyModifiedProperties()) {
				OnApplyModifiedProperties();
			}
		}

		#endregion Inspector
	}
}
